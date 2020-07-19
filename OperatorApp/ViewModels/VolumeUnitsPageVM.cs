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
using System.Threading.Tasks;

namespace OperatorApp.ViewModels
{
    public class VolumeUnitsPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ObservableCollection<MatchVolumeUnit> _volumeUnitsToMatch;
        public ObservableCollection<MatchVolumeUnit> VolumeUnitsToMatch
        {
            get { return _volumeUnitsToMatch; }
            set
            {
                _volumeUnitsToMatch = value;
                OnPropertyChanged("VolumeUnitsToMatch");
            }
        }

        private ObservableCollection<VolumeUnit> _volumeUnits;
        public ObservableCollection<VolumeUnit> VolumeUnits
        {
            get { return _volumeUnits; }
            set
            {
                _volumeUnits = value;
                OnPropertyChanged("VolumeUnits");
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

        private MatchVolumeUnit _selectedMatchVolumeUnit;
        public MatchVolumeUnit SelectedMatchVolumeUnit
        {
            get { return _selectedMatchVolumeUnit; }
            set
            {
                _selectedMatchVolumeUnit = value;

                if (VolumeUnits != null && _selectedMatchVolumeUnit != null)
                    SelectedVolumeUnit = VolumeUnits.Where(qu => qu.Id == _selectedMatchVolumeUnit.VolumeUnitId).FirstOrDefault();

                OnPropertyChanged("SelectedMatchVolumeUnit");
            }
        }

        private VolumeUnit _selectedVolumeUnit;
        public VolumeUnit SelectedVolumeUnit
        {
            get { return _selectedVolumeUnit; }
            set
            {
                _selectedVolumeUnit = value;
                OnPropertyChanged("SelectedVolumeUnit");
            }
        }

        private string _searchMatchVolumeUnitsText;
        public string SearchMatchVolumeUnitsText
        {
            get { return _searchMatchVolumeUnitsText; }
            set
            {
                _searchMatchVolumeUnitsText = value;
                OnPropertyChanged("SearchMatchVolumeUnitsText");
            }
        }

        private string _searchVolumeUnitsText;
        public string SearchVolumeUnitsText
        {
            get { return _searchVolumeUnitsText; }
            set
            {
                _searchVolumeUnitsText = value;
                OnPropertyChanged("SearchVolumeUnitsText");
            }
        }


        private void UpdateUnitsMatching()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                var ProductsToUpdate = db.Products
                    .Where(p => p.VolumeUnitId == SelectedMatchVolumeUnit.VolumeUnitId);
                foreach (Product product in ProductsToUpdate)
                    product.VolumeUnitId = SelectedVolumeUnit.Id;

                db.Products.UpdateRange(ProductsToUpdate);
                db.SaveChanges();
            }

            using (MarketDbContext db = new MarketDbContext())
            {

                SelectedMatchVolumeUnit.VolumeUnitId = SelectedVolumeUnit.Id;
                if (SelectedMatchVolumeUnit.IsChecked == false)
                {
                    SelectedMatchVolumeUnit.IsChecked = true;
                    UncheckedCount--;
                }
                db.MatchVolumeUnits.Update(SelectedMatchVolumeUnit);

                if (SelectedVolumeUnit.IsChecked == false)
                {
                    SelectedVolumeUnit.IsChecked = true;
                    db.VolumeUnits.Update(SelectedVolumeUnit);
                }
                db.SaveChanges();
            }
        }


        private async void MatchVolumeUnits()
        {
            int newListItemIndex = VolumeUnitsToMatch.IndexOf(SelectedMatchVolumeUnit);
            using (MarketDbContext db = new MarketDbContext())
            {
                VolumeUnit volumeUnitToRemove = await db.VolumeUnits.FindAsync(SelectedMatchVolumeUnit.VolumeUnitId);
                if (await db.MatchVolumeUnits.Where(mqu => mqu.VolumeUnitId == volumeUnitToRemove.Id).CountAsync() == 1 && volumeUnitToRemove.Id != SelectedVolumeUnit.Id)
                {
                    if (DialogService.ShowWarningMatchAndDeleteDialog(SelectedMatchVolumeUnit.Supplier.ShortName,
                        $"\"{SelectedMatchVolumeUnit.SupplierVUShortName}\" - \"{SelectedMatchVolumeUnit.SupplierVUFullName}\"",
                        $"\"{SelectedVolumeUnit.ShortName}\" - \"{SelectedVolumeUnit.FullName}\"",
                         $"\"{volumeUnitToRemove.ShortName}\" - \"{volumeUnitToRemove.FullName}\""))
                    {
                        UpdateUnitsMatching();
                        db.VolumeUnits.Remove(volumeUnitToRemove);
                        VolumeUnits.Remove(VolumeUnits.Where(qu => qu.Id == volumeUnitToRemove.Id).FirstOrDefault());
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    UpdateUnitsMatching();
                }
            }

            if (ShowUncheckedOnly)
            {
                VolumeUnitsToMatch.Remove(SelectedMatchVolumeUnit);
            }
            else
            {
                newListItemIndex++;
            }

            if (VolumeUnitsToMatch != null && newListItemIndex < VolumeUnitsToMatch.Count)
            {
                SelectedMatchVolumeUnit = VolumeUnitsToMatch[newListItemIndex];
            }
        }

        private async void EditVolumeUnit()
        {
            VolumeUnit updatedVolumeUnit = DialogService.ShowEditVolumeUnitDialog(SelectedVolumeUnit);
            if (updatedVolumeUnit != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedVolumeUnit.ShortName = updatedVolumeUnit.ShortName;
                    SelectedVolumeUnit.FullName = updatedVolumeUnit.FullName;
                    db.VolumeUnits.Update(SelectedVolumeUnit);
                    await db.SaveChangesAsync();
                }
            }
        }


        private async void QueryDb()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                VolumeUnitsToMatch = new ObservableCollection<MatchVolumeUnit>(await db.MatchVolumeUnits
                    .Include(mqu => mqu.Supplier)
                    .Where(mqu => ShowUncheckedOnly ? mqu.IsChecked == false : true)
                    .Where(mqu => mqu.SupplierVUShortName.Contains(SearchMatchVolumeUnitsText) || mqu.SupplierVUFullName.Contains(SearchMatchVolumeUnitsText) || mqu.Supplier.ShortName.Contains(SearchMatchVolumeUnitsText))
                    .AsNoTracking()
                    .ToListAsync()
                     );

                VolumeUnits = new ObservableCollection<VolumeUnit>(await db.VolumeUnits
                    .Where(qu => qu.ShortName.Contains(SearchVolumeUnitsText) || qu.FullName.Contains(SearchVolumeUnitsText))
                    .AsNoTracking()
                    .ToListAsync()
                    );

                UncheckedCount = await db.MatchVolumeUnits.Where(mqu => mqu.IsChecked == false).CountAsync();
            }
        }

        public CommandType SearchMatchVolumeUnitsCommand { get; }
        public CommandType CancelSearchMatchVolumeUnitsCommand { get; }

        public CommandType EditVolumeUnitCommand { get; }
        public CommandType SearchVolumeUnitsCommand { get; }
        public CommandType CancelSearchVolumeUnitsCommand { get; }

        public CommandType MatchVolumeUnitsCommand { get; }

        public CommandType ShowNextPageCommand { get; }

        public VolumeUnitsPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchVolumeUnitsText = "";
            SearchMatchVolumeUnitsText = "";

            SearchMatchVolumeUnitsCommand = new CommandType();
            SearchMatchVolumeUnitsCommand.Create(_ => QueryDb());
            CancelSearchMatchVolumeUnitsCommand = new CommandType();
            CancelSearchMatchVolumeUnitsCommand.Create(_ => { SearchMatchVolumeUnitsText = ""; QueryDb(); });

            EditVolumeUnitCommand = new CommandType();
            EditVolumeUnitCommand.Create(_ => EditVolumeUnit(), _ => SelectedVolumeUnit != null);
            SearchVolumeUnitsCommand = new CommandType();
            SearchVolumeUnitsCommand.Create(_ => QueryDb());
            CancelSearchVolumeUnitsCommand = new CommandType();
            CancelSearchVolumeUnitsCommand.Create(_ => { SearchVolumeUnitsText = ""; QueryDb(); });

            MatchVolumeUnitsCommand = new CommandType();
            MatchVolumeUnitsCommand.Create(_ => MatchVolumeUnits(), _ => SelectedVolumeUnit != null && SelectedMatchVolumeUnit != null);

            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowProductExtraPropertyTypesPage(), _ => UncheckedCount == 0);

            QueryDb();
        }

    }
}

