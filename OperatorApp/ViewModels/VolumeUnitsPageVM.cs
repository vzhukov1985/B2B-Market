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
                _ = QueryDb(true, false);
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

                if (VolumeUnits != null && _selectedMatchVolumeUnit != null && _selectedMatchVolumeUnit.VolumeUnitId != null)
                    SelectedVolumeUnit = VolumeUnits.Where(vu => vu.Id == _selectedMatchVolumeUnit.VolumeUnitId).FirstOrDefault();

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

        private async void RemoveMatchVolumeUnit()
        {
            List<Guid> unusedVolumeUnitsIds = MarketDbContext.GetUnusedMatchVolumeUnitsIds();
            if (unusedVolumeUnitsIds.Contains(SelectedMatchVolumeUnit.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>(SelectedMatchVolumeUnit.Supplier.ShortName, "\"" + SelectedMatchVolumeUnit.SupplierVUShortName + "\" - \"" + SelectedMatchVolumeUnit.SupplierVUFullName + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchVolumeUnits.Remove(SelectedMatchVolumeUnit);
                        await db.SaveChangesAsync();
                        _ = QueryDb(true, false);
                    }
                }
            }
            else
            {
                DialogService.ShowMessageDialog("Элемент не может быть удален, т.к. используется", "Удаление невозможно");
            }
        }

        private async void RemoveUnusedMatchVolumeUnits()
        {
            List<MatchVolumeUnit> unusedVolumeUnits = MarketDbContext.GetUnusedMatchVolumeUnits();
            if (unusedVolumeUnits.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedVolumeUnits.Select(vu => new Tuple<string, string>(vu.Supplier.ShortName, "\"" + vu.SupplierVUShortName + "\" - \"" + vu.SupplierVUFullName + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchVolumeUnits.RemoveRange(unusedVolumeUnits);
                        await db.SaveChangesAsync();
                        _ = QueryDb(true, false);
                    }
                }
            }
            else
            {
                DialogService.ShowMessageDialog("Все элементы в списке используются", "Удаление невозможно");
            }
        }

        private async void AddNewVolumeUnitBasedOnMatch()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                VolumeUnit volumeUnit = db.VolumeUnits.Where(vu => vu.ShortName == SelectedMatchVolumeUnit.SupplierVUShortName && vu.FullName == SelectedMatchVolumeUnit.SupplierVUFullName).FirstOrDefault();
                if (volumeUnit == null)
                {
                    volumeUnit = new VolumeUnit { Id = Guid.NewGuid(), ShortName = SelectedMatchVolumeUnit.SupplierVUShortName, FullName = SelectedMatchVolumeUnit.SupplierVUFullName };
                    db.VolumeUnits.Add(volumeUnit);
                    await db.SaveChangesAsync();
                    SelectedMatchVolumeUnit.VolumeUnitId = volumeUnit.Id;
                    db.MatchVolumeUnits.Update(SelectedMatchVolumeUnit);
                    await db.SaveChangesAsync();
                    _ = QueryDb();
                }
                else
                {
                    DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Проверьте и свяжите с ней или другой позицией", "Ошибка");
                    if (VolumeUnits.Where(vu => vu.ShortName == SelectedMatchVolumeUnit.SupplierVUShortName && vu.FullName == SelectedMatchVolumeUnit.SupplierVUFullName).FirstOrDefault() == null)
                    {
                        SearchVolumeUnitsText = "";
                    }
                    await QueryDb(false, true);
                    SelectedVolumeUnit = VolumeUnits.Where(vu => vu.ShortName == SelectedMatchVolumeUnit.SupplierVUShortName && vu.FullName == SelectedMatchVolumeUnit.SupplierVUFullName).FirstOrDefault();
                }
            }
        }

        private async void MatchVolumeUnits()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                SelectedMatchVolumeUnit.VolumeUnitId = SelectedVolumeUnit.Id;
                db.Update(SelectedMatchVolumeUnit);
                await db.SaveChangesAsync();
                _ = QueryDb(true, false);
            }
        }

        private async void AddVolumeUnit()
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
                    VolumeUnit newVolumeUnit = new VolumeUnit { Id = Guid.NewGuid(), ShortName = fields[0].Value, FullName = fields[1].Value };
                    if (db.VolumeUnits.Where(vu => vu.ShortName == newVolumeUnit.ShortName && vu.FullName == newVolumeUnit.FullName).FirstOrDefault() != null)
                    {
                        DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                        if (VolumeUnits.Where(vu => vu.ShortName == newVolumeUnit.ShortName && vu.FullName == newVolumeUnit.FullName).FirstOrDefault() == null)
                        {
                            SearchVolumeUnitsText = "";
                        }
                        await QueryDb(false, true);
                        SelectedVolumeUnit = VolumeUnits.Where(vu => vu.ShortName == newVolumeUnit.ShortName && vu.FullName == newVolumeUnit.FullName).FirstOrDefault();
                        return;
                    }

                    db.VolumeUnits.Add(newVolumeUnit);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void EditVolumeUnit()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Короткое название", SelectedVolumeUnit.ShortName),
                new ElementField("Полное название", SelectedVolumeUnit.FullName)
            };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedVolumeUnit.ShortName = fields[0].Value;
                    SelectedVolumeUnit.FullName = fields[1].Value;
                    db.VolumeUnits.Update(SelectedVolumeUnit);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void RemoveVolumeUnit()
        {
            List<Guid> unusedVolumeUnitsIds = MarketDbContext.GetUnusedVolumeUnitsIds();
            if (unusedVolumeUnitsIds.Contains(SelectedVolumeUnit.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>("", "\"" + SelectedVolumeUnit.ShortName + "\" - \"" + SelectedVolumeUnit.FullName + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.VolumeUnits.Remove(SelectedVolumeUnit);
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

        private async void RemoveUnusedVolumeUnits()
        {
            List<VolumeUnit> unusedVolumeUnits = MarketDbContext.GetUnusedVolumeUnits();
            if (unusedVolumeUnits.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedVolumeUnits.Select(vu => new Tuple<string, string>("", "\"" + vu.ShortName + "\" - \"" + vu.FullName + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.VolumeUnits.RemoveRange(unusedVolumeUnits);
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



        public async Task QueryDb(bool UpdateVolumeUnitsToMatch = true, bool UpdateVolumeUnits = true)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (UpdateVolumeUnitsToMatch)
                {
                    VolumeUnitsToMatch = new ObservableCollection<MatchVolumeUnit>(await db.MatchVolumeUnits
                        .Include(mvu => mvu.Supplier)
                        .Where(mvu => ShowUncheckedOnly ? mvu.VolumeUnitId == null : true)
                        .Where(mvu => SearchMatchVolumeUnitsText == null ? true : mvu.SupplierVUShortName.Contains(SearchMatchVolumeUnitsText) || mvu.SupplierVUFullName.Contains(SearchMatchVolumeUnitsText) || mvu.Supplier.ShortName.Contains(SearchMatchVolumeUnitsText))
                        .AsNoTracking()
                        .ToListAsync()
                         );
                    UncheckedCount = await db.MatchVolumeUnits.Where(mvu => mvu.VolumeUnitId == null).CountAsync();
                }

                if (UpdateVolumeUnits)
                {
                    VolumeUnits = new ObservableCollection<VolumeUnit>(await db.VolumeUnits
                        .Where(vu => SearchVolumeUnitsText == null ? true : vu.ShortName.Contains(SearchVolumeUnitsText) || vu.FullName.Contains(SearchVolumeUnitsText))
                        .AsNoTracking()
                        .ToListAsync()
                        );
                }
            }
        }

        public CommandType RemoveMatchVolumeUnitCommand { get; }
        public CommandType RemoveUnusedMatchVolumeUnitsCommand { get; }
        public CommandType SearchMatchVolumeUnitsCommand { get; }
        public CommandType CancelSearchMatchVolumeUnitsCommand { get; }

        public CommandType AddVolumeUnitCommand { get; }
        public CommandType EditVolumeUnitCommand { get; }
        public CommandType RemoveVolumeUnitCommand { get; }
        public CommandType RemoveUnusedVolumeUnitsCommand { get; }
        public CommandType SearchVolumeUnitsCommand { get; }
        public CommandType CancelSearchVolumeUnitsCommand { get; }

        public CommandType AddNewVolumeUnitBasedOnMatchCommand { get; }
        public CommandType MatchVolumeUnitsCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }
        public CommandType ShowNextPageCommand { get; }

        public VolumeUnitsPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchVolumeUnitsText = "";
            SearchMatchVolumeUnitsText = "";

            RemoveMatchVolumeUnitCommand = new CommandType();
            RemoveMatchVolumeUnitCommand.Create(_ => RemoveMatchVolumeUnit(), _ => SelectedMatchVolumeUnit != null);
            RemoveUnusedMatchVolumeUnitsCommand = new CommandType();
            RemoveUnusedMatchVolumeUnitsCommand.Create(_ => RemoveUnusedMatchVolumeUnits());
            SearchMatchVolumeUnitsCommand = new CommandType();
            SearchMatchVolumeUnitsCommand.Create(_ => _ = QueryDb(true, false));
            CancelSearchMatchVolumeUnitsCommand = new CommandType();
            CancelSearchMatchVolumeUnitsCommand.Create(_ => { SearchMatchVolumeUnitsText = ""; _ = QueryDb(true, false); }, _ => SearchMatchVolumeUnitsText != "");

            AddVolumeUnitCommand = new CommandType();
            AddVolumeUnitCommand.Create(_ => AddVolumeUnit());
            EditVolumeUnitCommand = new CommandType();
            EditVolumeUnitCommand.Create(_ => EditVolumeUnit(), _ => SelectedVolumeUnit != null);
            RemoveVolumeUnitCommand = new CommandType();
            RemoveVolumeUnitCommand.Create(_ => RemoveVolumeUnit(), _ => SelectedVolumeUnit != null);
            RemoveUnusedVolumeUnitsCommand = new CommandType();
            RemoveUnusedVolumeUnitsCommand.Create(_ => RemoveUnusedVolumeUnits());
            SearchVolumeUnitsCommand = new CommandType();
            SearchVolumeUnitsCommand.Create(_ => _ = QueryDb(false, true));
            CancelSearchVolumeUnitsCommand = new CommandType();
            CancelSearchVolumeUnitsCommand.Create(_ => { SearchVolumeUnitsText = ""; _ = QueryDb(false, true); }, _ => SearchVolumeUnitsText != "");

            AddNewVolumeUnitBasedOnMatchCommand = new CommandType();
            AddNewVolumeUnitBasedOnMatchCommand.Create(_ => AddNewVolumeUnitBasedOnMatch(), _ => SelectedMatchVolumeUnit != null);
            MatchVolumeUnitsCommand = new CommandType();
            MatchVolumeUnitsCommand.Create(_ => MatchVolumeUnits(), _ => SelectedVolumeUnit != null && SelectedMatchVolumeUnit != null);

            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowVolumeTypesPage());
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowProductExtraPropertyTypesPage());

            _ = QueryDb(false, true); //Query with true,false executes when ShowUncheckedOnly property is set
        }

    }
}
