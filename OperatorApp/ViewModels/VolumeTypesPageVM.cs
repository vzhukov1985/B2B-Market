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

        private MatchVolumeType _selectedMatchVolumeType;
        public MatchVolumeType SelectedMatchVolumeType
        {
            get { return _selectedMatchVolumeType; }
            set
            {
                _selectedMatchVolumeType = value;

                if (VolumeTypes != null && _selectedMatchVolumeType != null && _selectedMatchVolumeType.VolumeTypeId != null)
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

        private async void RemoveMatchVolumeType()
        {
            List<Guid> unusedVolumeTypesIds = MarketDbContext.GetUnusedMatchVolumeTypesIds();
            if (unusedVolumeTypesIds.Contains(SelectedMatchVolumeType.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>(SelectedMatchVolumeType.Supplier.ShortName, "\"" + SelectedMatchVolumeType.SupplierVolumeTypeName + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchVolumeTypes.Remove(SelectedMatchVolumeType);
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

        private async void RemoveUnusedMatchVolumeTypes()
        {
            List<MatchVolumeType> unusedVolumeTypes = MarketDbContext.GetUnusedMatchVolumeTypes();
            if (unusedVolumeTypes.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedVolumeTypes.Select(vt => new Tuple<string, string>(vt.Supplier.ShortName, "\"" + vt.SupplierVolumeTypeName + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchVolumeTypes.RemoveRange(unusedVolumeTypes);
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

        private async void AddNewVolumeTypeBasedOnMatch()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                VolumeType volumeType = db.VolumeTypes.Where(vt => vt.Name == SelectedMatchVolumeType.SupplierVolumeTypeName).FirstOrDefault();
                if (volumeType == null)
                {
                    volumeType = new VolumeType { Id = Guid.NewGuid(), Name = SelectedMatchVolumeType.SupplierVolumeTypeName };
                    db.VolumeTypes.Add(volumeType);
                    await db.SaveChangesAsync();
                    SelectedMatchVolumeType.VolumeTypeId = volumeType.Id;
                    db.MatchVolumeTypes.Update(SelectedMatchVolumeType);
                    await db.SaveChangesAsync();
                    _ = QueryDb();
                }
                else
                {
                    DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Проверьте и свяжите с ней или другой позицией", "Ошибка");
                    if (VolumeTypes.Where(vt => vt.Name == SelectedMatchVolumeType.SupplierVolumeTypeName).FirstOrDefault() == null)
                    {
                        SearchVolumeTypesText = "";
                    }
                    await QueryDb(false, true);
                    SelectedVolumeType = VolumeTypes.Where(vt => vt.Name == SelectedMatchVolumeType.SupplierVolumeTypeName).FirstOrDefault();
                }
            }
        }

        private async void MatchVolumeTypes()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                SelectedMatchVolumeType.VolumeTypeId = SelectedVolumeType.Id;
                db.Update(SelectedMatchVolumeType);
                await db.SaveChangesAsync();
                _ = QueryDb(true, false);
            }
        }

        private async void AddVolumeType()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", "")
            };

            fields = DialogService.ShowAddEditElementDlg(fields, false);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    VolumeType newVolumeType = new VolumeType { Id = Guid.NewGuid(), Name = fields[0].Value };
                    if (db.VolumeTypes.Where(vt => vt.Name == newVolumeType.Name).FirstOrDefault() != null)
                    {
                        DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                        if (VolumeTypes.Where(vt => vt.Name == newVolumeType.Name).FirstOrDefault() == null)
                        {
                            SearchVolumeTypesText = "";
                        }
                        await QueryDb(false, true);
                        SelectedVolumeType = VolumeTypes.Where(vt => vt.Name == newVolumeType.Name).FirstOrDefault();
                        return;
                    }

                    db.VolumeTypes.Add(newVolumeType);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void EditVolumeType()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", SelectedVolumeType.Name)
            };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedVolumeType.Name = fields[0].Value;
                    db.VolumeTypes.Update(SelectedVolumeType);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void RemoveVolumeType()
        {
            List<Guid> unusedVolumeTypesIds = MarketDbContext.GetUnusedVolumeTypesIds();
            if (unusedVolumeTypesIds.Contains(SelectedVolumeType.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>("", "\"" + SelectedVolumeType.Name + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.VolumeTypes.Remove(SelectedVolumeType);
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

        private async void RemoveUnusedVolumeTypes()
        {
            List<VolumeType> unusedVolumeTypes = MarketDbContext.GetUnusedVolumeTypes();
            if (unusedVolumeTypes.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedVolumeTypes.Select(vt => new Tuple<string, string>("", "\"" + vt.Name + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.VolumeTypes.RemoveRange(unusedVolumeTypes);
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



        public async Task QueryDb(bool UpdateVolumeTypesToMatch = true, bool UpdateVolumeTypes = true)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (UpdateVolumeTypesToMatch)
                {
                    VolumeTypesToMatch = new ObservableCollection<MatchVolumeType>(await db.MatchVolumeTypes
                        .Include(mvt => mvt.Supplier)
                        .Where(mvt => ShowUncheckedOnly ? mvt.VolumeTypeId == null : true)
                        .Where(mvt => SearchMatchVolumeTypesText == null ? true : mvt.SupplierVolumeTypeName.Contains(SearchMatchVolumeTypesText) || mvt.Supplier.ShortName.Contains(SearchMatchVolumeTypesText))
                        .AsNoTracking()
                        .ToListAsync()
                         );
                    UncheckedCount = await db.MatchVolumeTypes.Where(mvt => mvt.VolumeTypeId == null).CountAsync();
                }

                if (UpdateVolumeTypes)
                {
                    VolumeTypes = new ObservableCollection<VolumeType>(await db.VolumeTypes
                        .Where(vt => SearchVolumeTypesText == null ? true : vt.Name.Contains(SearchVolumeTypesText))
                        .AsNoTracking()
                        .ToListAsync()
                        );
                }
            }
        }

        public CommandType RemoveMatchVolumeTypeCommand { get; }
        public CommandType RemoveUnusedMatchVolumeTypesCommand { get; }
        public CommandType SearchMatchVolumeTypesCommand { get; }
        public CommandType CancelSearchMatchVolumeTypesCommand { get; }

        public CommandType AddVolumeTypeCommand { get; }
        public CommandType EditVolumeTypeCommand { get; }
        public CommandType RemoveVolumeTypeCommand { get; }
        public CommandType RemoveUnusedVolumeTypesCommand { get; }
        public CommandType SearchVolumeTypesCommand { get; }
        public CommandType CancelSearchVolumeTypesCommand { get; }

        public CommandType AddNewVolumeTypeBasedOnMatchCommand { get; }
        public CommandType MatchVolumeTypesCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }
        public CommandType ShowNextPageCommand { get; }

        public VolumeTypesPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchVolumeTypesText = "";
            SearchMatchVolumeTypesText = "";

            RemoveMatchVolumeTypeCommand = new CommandType();
            RemoveMatchVolumeTypeCommand.Create(_ => RemoveMatchVolumeType(), _ => SelectedMatchVolumeType != null);
            RemoveUnusedMatchVolumeTypesCommand = new CommandType();
            RemoveUnusedMatchVolumeTypesCommand.Create(_ => RemoveUnusedMatchVolumeTypes());
            SearchMatchVolumeTypesCommand = new CommandType();
            SearchMatchVolumeTypesCommand.Create(_ => _ = QueryDb(true, false));
            CancelSearchMatchVolumeTypesCommand = new CommandType();
            CancelSearchMatchVolumeTypesCommand.Create(_ => { SearchMatchVolumeTypesText = ""; _ = QueryDb(true, false); }, _ => SearchMatchVolumeTypesText != "");

            AddVolumeTypeCommand = new CommandType();
            AddVolumeTypeCommand.Create(_ => AddVolumeType());
            EditVolumeTypeCommand = new CommandType();
            EditVolumeTypeCommand.Create(_ => EditVolumeType(), _ => SelectedVolumeType != null);
            RemoveVolumeTypeCommand = new CommandType();
            RemoveVolumeTypeCommand.Create(_ => RemoveVolumeType(), _ => SelectedVolumeType != null);
            RemoveUnusedVolumeTypesCommand = new CommandType();
            RemoveUnusedVolumeTypesCommand.Create(_ => RemoveUnusedVolumeTypes());
            SearchVolumeTypesCommand = new CommandType();
            SearchVolumeTypesCommand.Create(_ => _ = QueryDb(false, true));
            CancelSearchVolumeTypesCommand = new CommandType();
            CancelSearchVolumeTypesCommand.Create(_ => { SearchVolumeTypesText = ""; _ = QueryDb(false, true); }, _ => SearchVolumeTypesText != "");

            AddNewVolumeTypeBasedOnMatchCommand = new CommandType();
            AddNewVolumeTypeBasedOnMatchCommand.Create(_ => AddNewVolumeTypeBasedOnMatch(), _ => SelectedMatchVolumeType != null);
            MatchVolumeTypesCommand = new CommandType();
            MatchVolumeTypesCommand.Create(_ => MatchVolumeTypes(), _ => SelectedVolumeType != null && SelectedMatchVolumeType != null);

            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowQuantityUnitsPage());
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowVolumeUnitsPage());

            _ = QueryDb(false, true); //Query with true,false executes when ShowUncheckedOnly property is set
        }

    }
}
