using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using OperatorApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace OperatorApp.ViewModels
{
    public class VolumeTypesPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;

        private ObservableCollection<MatchVolumeType> _volumeTypesToMatch;
        public ObservableCollection<MatchVolumeType> VolumeTypesToMatch
        {
            get { return _volumeTypesToMatch; }
            set
            {
                _volumeTypesToMatch = value;
                OnPropertyChanged("VolumeTypesToMatch");
            }
        }

        private ObservableCollection<VolumeType> _volumeTypes;
        public ObservableCollection<VolumeType> VolumeTypes
        {
            get { return _volumeTypes; }
            set
            {
                _volumeTypes = value;
                OnPropertyChanged("VolumeTypes");
            }
        }


        private bool _showUncheckedOnly;
        public bool ShowUncheckedOnly
        {
            get { return _showUncheckedOnly; }
            set
            {
                _showUncheckedOnly = value;
                QueryDb(true);
                OnPropertyChanged("ShowUncheckedOnly");
            }
        }

        private int _uncheckedCount;
        public int UncheckedCount
        {
            get { return _uncheckedCount; }
            set
            {
                _uncheckedCount = value;
                OnPropertyChanged("UncheckedCount");
            }
        }

        private MatchVolumeType _selectedMatchVolumeType;
        public MatchVolumeType SelectedMatchVolumeType
        {
            get { return _selectedMatchVolumeType; }
            set
            {
                _selectedMatchVolumeType = value;
                if (VolumeTypes != null && _selectedMatchVolumeType != null)
                    SelectedVolumeType = VolumeTypes.Where(vt => vt.Id == _selectedMatchVolumeType.VolumeTypeId).FirstOrDefault();
                OnPropertyChanged("SelectedMatchVolumeType");
            }
        }

        private VolumeType _selectedVolumeType;
        public VolumeType SelectedVolumeType
        {
            get { return _selectedVolumeType; }
            set
            {
                _selectedVolumeType = value;
                OnPropertyChanged("SelectedVolumeType");
            }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
            }
        }


        private void MatchVolumeTypes()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                var ProductsToUpdate = db.Products
                    .Where(p => p.VolumeTypeId == SelectedMatchVolumeType.VolumeTypeId);
                foreach (Product product in ProductsToUpdate)
                    product.VolumeTypeId = SelectedVolumeType.Id;

                db.Products.UpdateRange(ProductsToUpdate);
                db.SaveChanges();
            }

            using (MarketDbContext db = new MarketDbContext())
            {

                SelectedMatchVolumeType.VolumeTypeId = SelectedVolumeType.Id;
                SelectedMatchVolumeType.IsChecked = true;
                db.MatchVolumeTypes.Update(SelectedMatchVolumeType);
                SelectedVolumeType.IsChecked = true;
                db.VolumeTypes.Update(SelectedVolumeType);


                var volumeTypeToRemove = VolumeTypes.Where(vt => vt.Name == SelectedMatchVolumeType.SupplierVolumeTypeName).FirstOrDefault();
                if (volumeTypeToRemove != null)
                {
                    var howManyMatchedTypesReferToTypeToRemove = VolumeTypesToMatch.Where(mvt => mvt.VolumeTypeId == volumeTypeToRemove.Id).Count();
                    if (howManyMatchedTypesReferToTypeToRemove == 0)
                    {
                        db.VolumeTypes.Remove(volumeTypeToRemove);
                        VolumeTypes.Remove(volumeTypeToRemove);
                    }
                }
                if (ShowUncheckedOnly)
                    VolumeTypesToMatch.Remove(SelectedMatchVolumeType);

                UncheckedCount = VolumeTypesToMatch.Where(mvt => mvt.IsChecked == false).Count();

                db.SaveChanges();
            }
        }


        public void QueryDb(bool reloadQuantityUnitsToMatch)
        {
            using (MarketDbContext db = new MarketDbContext())
            {

                if (reloadQuantityUnitsToMatch)
                {
                    VolumeTypesToMatch = new ObservableCollection<MatchVolumeType>(db.MatchVolumeTypes
                        .AsNoTracking()
                        .Where(mvt => ShowUncheckedOnly ? mvt.IsChecked == false : true)
                        .Include(mvt => mvt.Supplier)
                         );
                }

                VolumeTypes = new ObservableCollection<VolumeType>(db.VolumeTypes
                    .AsNoTracking()
                    .Where(vt => vt.Name.Contains(SearchText))
                    );

                UncheckedCount = VolumeTypesToMatch.Where(mqu => mqu.IsChecked == false).Count();
            }
        }

        public CommandType MatchVolumeTypesCommand { get; }
        public CommandType SearchCommand { get; }
        public CommandType CancelSearchCommand { get; }

        public CommandType ShowNextPageCommand { get; }
        public VolumeTypesPageVM(IPageService pageService)
        {
            PageService = pageService;

            ShowUncheckedOnly = true;
            SearchText = "";

            MatchVolumeTypesCommand = new CommandType();
            MatchVolumeTypesCommand.Create(_ => MatchVolumeTypes(), _ => SelectedVolumeType != null && SelectedMatchVolumeType != null);
            SearchCommand = new CommandType();
            SearchCommand.Create(_ => QueryDb(false));
            CancelSearchCommand = new CommandType();
            CancelSearchCommand.Create(_ => { SearchText = ""; QueryDb(false); });
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowVolumeTypesPage(), _ => UncheckedCount == 0);

            QueryDb(true);
        }
    }
}
