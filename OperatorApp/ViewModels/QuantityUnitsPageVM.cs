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
    public class QuantityUnitsPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ObservableCollection<MatchQuantityUnit> _quantityUnitsToMatch;
        public ObservableCollection<MatchQuantityUnit> QuantityUnitsToMatch
        {
            get { return _quantityUnitsToMatch; }
            set
            {
                _quantityUnitsToMatch = value;
                OnPropertyChanged("QuantityUnitsToMatch");
            }
        }

        private ObservableCollection<QuantityUnit> _quantityUnits;
        public ObservableCollection<QuantityUnit> QuantityUnits
        {
            get { return _quantityUnits; }
            set
            {
                _quantityUnits = value;
                OnPropertyChanged("QuantityUnits");
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

        private MatchQuantityUnit _selectedMatchQuantityUnit;
        public MatchQuantityUnit SelectedMatchQuantityUnit
        {
            get { return _selectedMatchQuantityUnit; }
            set
            {
                _selectedMatchQuantityUnit = value;

                if (QuantityUnits != null && _selectedMatchQuantityUnit != null)
                    SelectedQuantityUnit = QuantityUnits.Where(qu => qu.Id == _selectedMatchQuantityUnit.QuantityUnitId).FirstOrDefault();
                OnPropertyChanged("SelectedMatchQuantityUnit");
            }
        }

        private QuantityUnit _selectedQuantityUnit;
        public QuantityUnit SelectedQuantityUnit
        {
            get { return _selectedQuantityUnit; }
            set
            {
                _selectedQuantityUnit = value;
                OnPropertyChanged("SelectedQuantityUnit");
            }
        }

        private string _searchMatchQuantityUnitsText;
        public string SearchMatchQuantityUnitsText
        {
            get { return _searchMatchQuantityUnitsText; }
            set
            {
                _searchMatchQuantityUnitsText = value;
                OnPropertyChanged("SearchMatchQuantityUnitsText");
            }
        }

        private string _searchQuantityUnitsText;
        public string SearchQuantityUnitsText
        {
            get { return _searchQuantityUnitsText; }
            set
            {
                _searchQuantityUnitsText = value;
                OnPropertyChanged("SearchQuantityUnitsText");
            }
        }


        private void UpdateUnitsMatching()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                var offersToUpdate = db.Offers
                    .Where(of => of.QuantityUnitId == SelectedMatchQuantityUnit.QuantityUnitId);
                foreach (Offer offer in offersToUpdate)
                    offer.QuantityUnitId = SelectedQuantityUnit.Id;

                db.Offers.UpdateRange(offersToUpdate);
                db.SaveChanges();
            }

            using (MarketDbContext db = new MarketDbContext())
            {

                SelectedMatchQuantityUnit.QuantityUnitId = SelectedQuantityUnit.Id;
                if (SelectedMatchQuantityUnit.IsChecked == false)
                {
                    SelectedMatchQuantityUnit.IsChecked = true;
                    UncheckedCount--;
                }
                db.MatchQuantityUnits.Update(SelectedMatchQuantityUnit);

                SelectedQuantityUnit.IsChecked = true;
                db.QuantityUnits.Update(SelectedQuantityUnit);
                db.SaveChanges();
            }
        }


        private void MatchQuantityUnits()
        {
            int newListItemIndex = QuantityUnitsToMatch.IndexOf(SelectedMatchQuantityUnit);
            using (MarketDbContext db = new MarketDbContext())
            {
                QuantityUnit quantityUnitToRemove = db.QuantityUnits.Find(SelectedMatchQuantityUnit.QuantityUnitId);
                if (db.MatchQuantityUnits.Where(mqu => mqu.QuantityUnitId == quantityUnitToRemove.Id).Count() == 1)
                {
                    if (DialogService.ShowWarningQuantityUnitDialog(SelectedMatchQuantityUnit.Supplier.ShortName,
                        $"\"{SelectedMatchQuantityUnit.SupplierQUShortName}\" - \"{SelectedMatchQuantityUnit.SupplierQUFullName}\"",
                        $"\"{SelectedQuantityUnit.ShortName}\" - \"{SelectedQuantityUnit.FullName}\"",
                         $"\"{quantityUnitToRemove.ShortName}\" - \"{quantityUnitToRemove.FullName}\""))
                    {
                        UpdateUnitsMatching();
                        db.QuantityUnits.Remove(quantityUnitToRemove);
                        QuantityUnits.Remove(QuantityUnits.Where(qu => qu.Id == quantityUnitToRemove.Id).FirstOrDefault());
                        db.SaveChanges();
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
                QuantityUnitsToMatch.Remove(SelectedMatchQuantityUnit);
            }
            else
            {
                newListItemIndex++;
            }

            if (QuantityUnitsToMatch != null && newListItemIndex < QuantityUnitsToMatch.Count)
            {
                SelectedMatchQuantityUnit = QuantityUnitsToMatch[newListItemIndex];
            }
        }

        private void EditQuantityUnit()
        {
            QuantityUnit updatedQuantityUnit = DialogService.ShowEditQuantityUnitDialog(SelectedQuantityUnit);
            if (updatedQuantityUnit != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedQuantityUnit.ShortName = updatedQuantityUnit.ShortName;
                    SelectedQuantityUnit.FullName = updatedQuantityUnit.FullName;
                    db.QuantityUnits.Update(SelectedQuantityUnit);
                    db.SaveChanges();
                }
            }
        }


        private void QueryDb()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                QuantityUnitsToMatch = new ObservableCollection<MatchQuantityUnit>(db.MatchQuantityUnits
                    .Include(mqu => mqu.Supplier)
                    .Where(mqu => ShowUncheckedOnly ? mqu.IsChecked == false : true)
                    .Where(mqu => mqu.SupplierQUShortName.Contains(SearchMatchQuantityUnitsText) || mqu.SupplierQUFullName.Contains(SearchMatchQuantityUnitsText) || mqu.Supplier.ShortName.Contains(SearchMatchQuantityUnitsText))
                    .AsNoTracking()
                     );

                QuantityUnits = new ObservableCollection<QuantityUnit>(db.QuantityUnits
                    .Where(qu => qu.ShortName.Contains(SearchQuantityUnitsText) || qu.FullName.Contains(SearchQuantityUnitsText))
                    .AsNoTracking()
                    );

                UncheckedCount = db.MatchQuantityUnits.Where(mqu => mqu.IsChecked == false).Count();
            }
        }

        public CommandType SearchMatchQuantityUnitsCommand { get; }
        public CommandType CancelSearchMatchQuantityUnitsCommand { get; }

        public CommandType EditQuantityUnitCommand { get; }
        public CommandType SearchQuantityUnitsCommand { get; }
        public CommandType CancelSearchQuantityUnitsCommand { get; }

        public CommandType MatchQuantityUnitsCommand { get; }

        public CommandType ShowNextPageCommand { get; }

        public QuantityUnitsPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchQuantityUnitsText = "";
            SearchMatchQuantityUnitsText = "";

            SearchMatchQuantityUnitsCommand = new CommandType();
            SearchMatchQuantityUnitsCommand.Create(_ => QueryDb());
            CancelSearchMatchQuantityUnitsCommand = new CommandType();
            CancelSearchMatchQuantityUnitsCommand.Create(_ => { SearchMatchQuantityUnitsText = ""; QueryDb(); });

            EditQuantityUnitCommand = new CommandType();
            EditQuantityUnitCommand.Create(_ => EditQuantityUnit(), _ => SelectedQuantityUnit != null);
            SearchQuantityUnitsCommand = new CommandType();
            SearchQuantityUnitsCommand.Create(_ => QueryDb());
            CancelSearchQuantityUnitsCommand = new CommandType();
            CancelSearchQuantityUnitsCommand.Create(_ => { SearchQuantityUnitsText = ""; QueryDb(); });
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowVolumeTypesPage(), _ => UncheckedCount == 0);

            MatchQuantityUnitsCommand = new CommandType();
            MatchQuantityUnitsCommand.Create(_ => MatchQuantityUnits(), _ => SelectedQuantityUnit != null && SelectedMatchQuantityUnit != null);

            QueryDb();
        }

    }
}
