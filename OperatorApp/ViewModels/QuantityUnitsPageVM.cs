using Core.DBModels;
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
                _ = QueryDb(true,false);
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

                if (QuantityUnits != null && _selectedMatchQuantityUnit != null && _selectedMatchQuantityUnit.QuantityUnitId != null)
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

        private async void RemoveMatchQuantityUnit()
        {
            List<Guid> unusedQuantityUnitsIds = MarketDbContext.GetUnusedMatchQuantityUnitsIds();
            if (unusedQuantityUnitsIds.Contains(SelectedMatchQuantityUnit.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>(SelectedMatchQuantityUnit.Supplier.ShortName, "\"" + SelectedMatchQuantityUnit.SupplierQUShortName + "\" - \"" + SelectedMatchQuantityUnit.SupplierQUFullName + "\"" );
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchQuantityUnits.Remove(SelectedMatchQuantityUnit);
                        await db.SaveChangesAsync();
                        _ = QueryDb(true,false);
                    }

                }
            }
            else
            {
                DialogService.ShowMessageDialog("Элемент не может быть удален, т.к. используется", "Удаление невозможно");
            }
        }

        private async void RemoveUnusedMatchQuantityUnits()
        {
            List<MatchQuantityUnit> unusedQuantityUnits = MarketDbContext.GetUnusedMatchQuantityUnits();
            if (unusedQuantityUnits.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedQuantityUnits.Select(qu => new Tuple<string, string>(qu.Supplier.ShortName, "\"" + qu.SupplierQUShortName + "\" - \"" + qu.SupplierQUFullName + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchQuantityUnits.RemoveRange(unusedQuantityUnits);
                        await db.SaveChangesAsync();
                        _ = QueryDb(true,false);
                    }
                }
            }
            else
            {
                DialogService.ShowMessageDialog("Все элементы в списке используются", "Удаление невозможно");
            }
        }

        private async void AddNewQuantityUnitBasedOnMatch()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                QuantityUnit quantityUnit = db.QuantityUnits.Where(qu => qu.ShortName == SelectedMatchQuantityUnit.SupplierQUShortName && qu.FullName == SelectedMatchQuantityUnit.SupplierQUFullName).FirstOrDefault();
                if (quantityUnit == null)
                {
                    quantityUnit = new QuantityUnit { Id = Guid.NewGuid(), ShortName = SelectedMatchQuantityUnit.SupplierQUShortName, FullName = SelectedMatchQuantityUnit.SupplierQUFullName };
                    db.QuantityUnits.Add(quantityUnit);
                    await db.SaveChangesAsync();
                    SelectedMatchQuantityUnit.QuantityUnitId = quantityUnit.Id;
                    db.MatchQuantityUnits.Update(SelectedMatchQuantityUnit);
                    await db.SaveChangesAsync();
                    _= QueryDb();
                }
                else
                {
                    DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Проверьте и свяжите с ней или другой позицией", "Ошибка");
                    if (QuantityUnits.Where(qu => qu.ShortName == SelectedMatchQuantityUnit.SupplierQUShortName && qu.FullName == SelectedMatchQuantityUnit.SupplierQUFullName).FirstOrDefault() == null)
                    {
                        SearchQuantityUnitsText = "";
                    }
                    await QueryDb(false,true);
                    SelectedQuantityUnit = QuantityUnits.Where(qu => qu.ShortName == SelectedMatchQuantityUnit.SupplierQUShortName && qu.FullName == SelectedMatchQuantityUnit.SupplierQUFullName).FirstOrDefault();
                }
            }
        }

        private async void MatchQuantityUnits()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                SelectedMatchQuantityUnit.QuantityUnitId = SelectedQuantityUnit.Id;
                db.Update(SelectedMatchQuantityUnit);
                await db.SaveChangesAsync();
                _ = QueryDb(true,false);
            }
        }

        private async void AddQuantityUnit()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Короткое название", ""),
                new ElementField("Полное название", "")
            };

            fields = DialogService.ShowAddEditElementDlg(fields, false);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    QuantityUnit newQuantityUnit = new QuantityUnit { Id = Guid.NewGuid(), ShortName = fields[0].Value, FullName = fields[1].Value };
                    if (db.QuantityUnits.Where(qu => qu.ShortName == newQuantityUnit.ShortName && qu.FullName == newQuantityUnit.FullName).FirstOrDefault() != null)
                    {
                        DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                        if (QuantityUnits.Where(qu => qu.ShortName == newQuantityUnit.ShortName && qu.FullName == newQuantityUnit.FullName).FirstOrDefault() == null)
                        {
                            SearchQuantityUnitsText = "";
                        }
                        await QueryDb(false, true);
                        SelectedQuantityUnit = QuantityUnits.Where(qu => qu.ShortName == newQuantityUnit.ShortName && qu.FullName == newQuantityUnit.FullName).FirstOrDefault();
                        return;
                    }

                    db.QuantityUnits.Add(newQuantityUnit);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void EditQuantityUnit()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Короткое название", SelectedQuantityUnit.ShortName),
                new ElementField("Полное название", SelectedQuantityUnit.FullName)
            };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedQuantityUnit.ShortName = fields[0].Value;
                    SelectedQuantityUnit.FullName = fields[1].Value;
                    db.QuantityUnits.Update(SelectedQuantityUnit);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void RemoveQuantityUnit()
        {
            List<Guid> unusedQuantityUnitsIds = MarketDbContext.GetUnusedQuantityUnitsIds();
            if (unusedQuantityUnitsIds.Contains(SelectedQuantityUnit.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>("", "\"" + SelectedQuantityUnit.ShortName + "\" - \"" + SelectedQuantityUnit.FullName + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.QuantityUnits.Remove(SelectedQuantityUnit);
                        await db.SaveChangesAsync();
                        _ = QueryDb(false, true);
                    }
                }
            }
            else
            {
                DialogService.ShowMessageDialog("Элемент не может быть удален, т.к. используется", "Удаление невозможно");
            }
        }

        private async void RemoveUnusedQuantityUnits()
        {
            List<QuantityUnit> unusedQuantityUnits = MarketDbContext.GetUnusedQuantityUnits();
            if (unusedQuantityUnits.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedQuantityUnits.Select(qu => new Tuple<string, string>("", "\"" + qu.ShortName + "\" - \"" + qu.FullName + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.QuantityUnits.RemoveRange(unusedQuantityUnits);
                        await db.SaveChangesAsync();
                        _ = QueryDb(false, true);
                    }
                }
            }
            else
            {
                DialogService.ShowMessageDialog("Все элементы в списке используются", "Удаление невозможно");
            }
        }


        public async Task QueryDb(bool UpdateQuantityUnitsToMatch = true, bool UpdateQuantityUnits = true)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (UpdateQuantityUnitsToMatch)
                {
                    QuantityUnitsToMatch = new ObservableCollection<MatchQuantityUnit>(await db.MatchQuantityUnits
                        .Include(mqu => mqu.Supplier)
                        .Where(mqu => ShowUncheckedOnly ? mqu.QuantityUnitId == null : true)
                        .Where(mqu => SearchMatchQuantityUnitsText == null ? true :
                            EF.Functions.Like(mqu.SupplierQUShortName, $"%{SearchMatchQuantityUnitsText}%") ||
                            EF.Functions.Like(mqu.SupplierQUFullName, $"%{SearchMatchQuantityUnitsText}%") ||
                            EF.Functions.Like(mqu.Supplier.ShortName, $"%{SearchMatchQuantityUnitsText}%"))
                        .AsNoTracking()
                        .ToListAsync()
                         );
                    UncheckedCount = await db.MatchQuantityUnits.Where(mqu => mqu.QuantityUnitId == null).CountAsync();
                }

                if (UpdateQuantityUnits)
                {
                    QuantityUnits = new ObservableCollection<QuantityUnit>(await db.QuantityUnits
                        .Where(qu => SearchQuantityUnitsText == null ? true :
                            EF.Functions.Like(qu.ShortName, $"%{SearchQuantityUnitsText}%") ||
                            EF.Functions.Like(qu.FullName, $"%{SearchQuantityUnitsText}%"))
                        .AsNoTracking()
                        .ToListAsync()
                        );
                }
            }
        }

        public CommandType RemoveMatchQuantityUnitCommand { get; }
        public CommandType RemoveUnusedMatchQuantityUnitsCommand { get; }
        public CommandType SearchMatchQuantityUnitsCommand { get; }
        public CommandType CancelSearchMatchQuantityUnitsCommand { get; }

        public CommandType AddQuantityUnitCommand { get; }
        public CommandType EditQuantityUnitCommand { get; }
        public CommandType RemoveQuantityUnitCommand { get; }
        public CommandType RemoveUnusedQuantityUnitsCommand { get; }
        public CommandType SearchQuantityUnitsCommand { get; }
        public CommandType CancelSearchQuantityUnitsCommand { get; }

        public CommandType AddNewQuantityUnitBasedOnMatchCommand { get; }
        public CommandType MatchQuantityUnitsCommand { get; }

        public CommandType ShowNextPageCommand { get; }

        public CommandType ShowPositionOffersCommand { get; }
        public CommandType ShowMatchedPositionDependenciesCommand { get; }

        public QuantityUnitsPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchQuantityUnitsText = "";
            SearchMatchQuantityUnitsText = "";

            RemoveMatchQuantityUnitCommand = new CommandType();
            RemoveMatchQuantityUnitCommand.Create(_ => RemoveMatchQuantityUnit(), _ => SelectedMatchQuantityUnit != null);
            RemoveUnusedMatchQuantityUnitsCommand = new CommandType();
            RemoveUnusedMatchQuantityUnitsCommand.Create(_ => RemoveUnusedMatchQuantityUnits());
            SearchMatchQuantityUnitsCommand = new CommandType();
            SearchMatchQuantityUnitsCommand.Create(_ => _ = QueryDb(true,false));
            CancelSearchMatchQuantityUnitsCommand = new CommandType();
            CancelSearchMatchQuantityUnitsCommand.Create(_ => { SearchMatchQuantityUnitsText = ""; _ = QueryDb(true,false); }, _ => SearchMatchQuantityUnitsText!="");

            AddQuantityUnitCommand = new CommandType();
            AddQuantityUnitCommand.Create(_ => AddQuantityUnit());
            EditQuantityUnitCommand = new CommandType();
            EditQuantityUnitCommand.Create(_ => EditQuantityUnit(), _ => SelectedQuantityUnit != null);
            RemoveQuantityUnitCommand = new CommandType();
            RemoveQuantityUnitCommand.Create(_ => RemoveQuantityUnit(), _ => SelectedQuantityUnit != null);
            RemoveUnusedQuantityUnitsCommand = new CommandType();
            RemoveUnusedQuantityUnitsCommand.Create(_ => RemoveUnusedQuantityUnits());
            SearchQuantityUnitsCommand = new CommandType();
            SearchQuantityUnitsCommand.Create(_ => _ = QueryDb(false, true));
            CancelSearchQuantityUnitsCommand = new CommandType();
            CancelSearchQuantityUnitsCommand.Create(_ => { SearchQuantityUnitsText = ""; _ = QueryDb(false,true); }, _ => SearchQuantityUnitsText!="");

            AddNewQuantityUnitBasedOnMatchCommand = new CommandType();
            AddNewQuantityUnitBasedOnMatchCommand.Create(_ => AddNewQuantityUnitBasedOnMatch(), _ => SelectedMatchQuantityUnit != null);
            MatchQuantityUnitsCommand = new CommandType();
            MatchQuantityUnitsCommand.Create(_ => MatchQuantityUnits(), _ => SelectedQuantityUnit != null && SelectedMatchQuantityUnit != null);

            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowVolumeTypesPage());

            ShowPositionOffersCommand = new CommandType();
            ShowPositionOffersCommand.Create(_ => DialogService.ShowPositionOffers(SelectedMatchQuantityUnit), _ => SelectedMatchQuantityUnit != null);
            ShowMatchedPositionDependenciesCommand = new CommandType();
            ShowMatchedPositionDependenciesCommand.Create(_ => DialogService.ShowPositionOffers(SelectedQuantityUnit), _ => SelectedQuantityUnit != null);

            _ = QueryDb(false, true); //Query with true,false executes when ShowUncheckedOnly property is set
        }

    }
}
