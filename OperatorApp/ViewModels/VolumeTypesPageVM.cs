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
        private readonly IDialogService DialogService;

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
                QueryDb();
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

        private string _searchMatchVolumeTypesText;
        public string SearchMatchVolumeTypesText
        {
            get { return _searchMatchVolumeTypesText; }
            set
            {
                _searchMatchVolumeTypesText = value;
                OnPropertyChanged("SearchMatchVolumeTypesText");
            }
        }

        private string _searchVolumeTypesText;
        public string SearchVolumeTypesText
        {
            get { return _searchVolumeTypesText; }
            set
            {
                _searchVolumeTypesText = value;
                OnPropertyChanged("SearchVolumeTypesText");
            }
        }


        private void UpdateTypesMatching()
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
                if (SelectedMatchVolumeType.IsChecked == false)
                {
                    SelectedMatchVolumeType.IsChecked = true;
                    UncheckedCount--;
                }
                db.MatchVolumeTypes.Update(SelectedMatchVolumeType);

                if (SelectedVolumeType.IsChecked == false)
                {
                    SelectedVolumeType.IsChecked = true;
                    db.VolumeTypes.Update(SelectedVolumeType);
                }
                db.SaveChanges();
            }
        }


        private async void MatchVolumeTypes()
        {
            int newListItemIndex = VolumeTypesToMatch.IndexOf(SelectedMatchVolumeType);
            using (MarketDbContext db = new MarketDbContext())
            {
                VolumeType volumeTypeToRemove = await db.VolumeTypes.FindAsync(SelectedMatchVolumeType.VolumeTypeId);
                if (await db.MatchVolumeTypes.Where(mvt => mvt.VolumeTypeId == volumeTypeToRemove.Id).CountAsync() == 1 && volumeTypeToRemove.Id != SelectedVolumeType.Id)
                {
                    if (DialogService.ShowWarningMatchAndDeleteDialog(SelectedMatchVolumeType.Supplier.ShortName,
                        $"\"{SelectedMatchVolumeType.SupplierVolumeTypeName}\"",
                        $"\"{SelectedVolumeType.Name}\"",
                         $"\"{volumeTypeToRemove.Name}\""))
                    {
                        UpdateTypesMatching();
                        db.VolumeTypes.Remove(volumeTypeToRemove);
                        VolumeTypes.Remove(VolumeTypes.Where(vt => vt.Id == volumeTypeToRemove.Id).FirstOrDefault());
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    UpdateTypesMatching();
                }
            }

            if (ShowUncheckedOnly)
            {
                VolumeTypesToMatch.Remove(SelectedMatchVolumeType);
            }
            else
            {
                newListItemIndex++;
            }

            if (VolumeTypesToMatch != null && newListItemIndex < VolumeTypesToMatch.Count)
            {
                SelectedMatchVolumeType = VolumeTypesToMatch[newListItemIndex];
            }
        }

        private async void EditVolumeType()
        {
            VolumeType updatedVolumeType = DialogService.ShowEditVolumeTypeDialog(SelectedVolumeType);
            if (updatedVolumeType != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedVolumeType.Name = updatedVolumeType.Name;
                    db.VolumeTypes.Update(SelectedVolumeType);
                    await db.SaveChangesAsync();
                }
            }
        }



        public async void QueryDb()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                VolumeTypesToMatch = new ObservableCollection<MatchVolumeType>(await db.MatchVolumeTypes
                    .Include(mvt => mvt.Supplier)
                    .Where(mvt => ShowUncheckedOnly ? mvt.IsChecked == false : true)
                    .Where(mvt => mvt.SupplierVolumeTypeName.Contains(SearchMatchVolumeTypesText) || mvt.Supplier.ShortName.Contains(SearchMatchVolumeTypesText))
                    .AsNoTracking()
                    .ToListAsync()
                    );

                VolumeTypes = new ObservableCollection<VolumeType>(await db.VolumeTypes
                    .Where(vt => vt.Name.Contains(SearchVolumeTypesText))
                    .AsNoTracking()
                    .ToListAsync()
                    );

                UncheckedCount = await db.MatchVolumeTypes.Where(mvt => mvt.IsChecked == false).CountAsync();
            }
        }

        public CommandType SearchMatchVolumeTypesCommand { get; }
        public CommandType CancelSearchMatchVolumeTypesCommand { get; }

        public CommandType EditVolumeTypeCommand { get; }
        public CommandType SearchVolumeTypesCommand { get; }
        public CommandType CancelSearchVolumeTypesCommand { get; }

        public CommandType MatchVolumeTypesCommand { get; }

        public CommandType ShowNextPageCommand { get; }

        public VolumeTypesPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchVolumeTypesText = "";
            SearchMatchVolumeTypesText = "";

            SearchMatchVolumeTypesCommand = new CommandType();
            SearchMatchVolumeTypesCommand.Create(_ => QueryDb());
            CancelSearchMatchVolumeTypesCommand = new CommandType();
            CancelSearchMatchVolumeTypesCommand.Create(_ => { SearchMatchVolumeTypesText = ""; QueryDb(); });

            EditVolumeTypeCommand = new CommandType();
            EditVolumeTypeCommand.Create(_ => EditVolumeType(), _ => SelectedVolumeType != null);
            SearchVolumeTypesCommand = new CommandType();
            SearchVolumeTypesCommand.Create(_ => QueryDb());
            CancelSearchVolumeTypesCommand = new CommandType();
            CancelSearchVolumeTypesCommand.Create(_ => { SearchVolumeTypesText = ""; QueryDb(); });

            MatchVolumeTypesCommand = new CommandType();
            MatchVolumeTypesCommand.Create(_ => MatchVolumeTypes(), _ => SelectedVolumeType != null && SelectedMatchVolumeType != null);

            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowVolumeUnitsPage(), _ => UncheckedCount == 0);

            QueryDb();
        }
    }
}
