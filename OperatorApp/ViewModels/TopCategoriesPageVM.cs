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
    public class TopCategoriesPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ObservableCollection<MidCategory> _midCategoriesToMatch;
        public ObservableCollection<MidCategory> MidCategories
        {
            get { return _midCategoriesToMatch; }
            set
            {
                _midCategoriesToMatch = value;
                OnPropertyChanged("MidCategories");
            }
        }

        private ObservableCollection<TopCategory> _topCategories;
        public ObservableCollection<TopCategory> TopCategories
        {
            get { return _topCategories; }
            set
            {
                _topCategories = value;
                OnPropertyChanged("TopCategories");
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

        private MidCategory _selectedMidCategory;
        public MidCategory SelectedMidCategory
        {
            get { return _selectedMidCategory; }
            set
            {
                _selectedMidCategory = value;

                if (TopCategories != null && _selectedMidCategory != null && _selectedMidCategory.TopCategoryId != null)
                    SelectedTopCategory = TopCategories.Where(vt => vt.Id == _selectedMidCategory.TopCategoryId).FirstOrDefault();

                OnPropertyChanged("SelectedMidCategory");
            }
        }

        private TopCategory _selectedTopCategory;
        public TopCategory SelectedTopCategory
        {
            get { return _selectedTopCategory; }
            set
            {
                _selectedTopCategory = value;
                OnPropertyChanged("SelectedTopCategory");
            }
        }

        private string _searchMidCategoriesText;
        public string SearchMidCategoriesText
        {
            get { return _searchMidCategoriesText; }
            set
            {
                _searchMidCategoriesText = value;
                OnPropertyChanged("SearchMidCategoriesText");
            }
        }

        private string _searchTopCategoriesText;
        public string SearchTopCategoriesText
        {
            get { return _searchTopCategoriesText; }
            set
            {
                _searchTopCategoriesText = value;
                OnPropertyChanged("SearchTopCategoriesText");
            }
        }

        private async void RemoveMidCategory()
        {
            List<Guid> unusedCategoriesIds = MarketDbContext.GetUnusedMidCategoriesIds();
            if (unusedCategoriesIds.Contains(SelectedMidCategory.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>("", "\"" + SelectedMidCategory.Name + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MidCategories.Remove(SelectedMidCategory);
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

        private async void RemoveUnusedMidCategories()
        {
            List<MidCategory> unusedCategories = MarketDbContext.GetUnusedMidCategories();
            if (unusedCategories.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedCategories.Select(vt => new Tuple<string, string>("", "\"" + vt.Name + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MidCategories.RemoveRange(unusedCategories);
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

        private async void MatchMidCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                SelectedMidCategory.TopCategoryId = SelectedTopCategory.Id;
                db.Update(SelectedMidCategory);
                await db.SaveChangesAsync();
                _ = QueryDb(true, false);
            }
        }

        private async void AddTopCategory()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", "")
            };

            fields = DialogService.ShowAddEditElementDlg(fields, false);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    TopCategory newTopCategory = new TopCategory { Id = Guid.NewGuid(), Name = fields[0].Value };
                    if (db.TopCategories.Where(vt => vt.Name == newTopCategory.Name).FirstOrDefault() != null)
                    {
                        DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                        if (TopCategories.Where(vt => vt.Name == newTopCategory.Name).FirstOrDefault() == null)
                        {
                            SearchTopCategoriesText = "";
                        }
                        await QueryDb(false, true);
                        SelectedTopCategory = TopCategories.Where(vt => vt.Name == newTopCategory.Name).FirstOrDefault();
                        return;
                    }

                    db.TopCategories.Add(newTopCategory);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void EditTopCategory()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", SelectedTopCategory.Name)
            };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedTopCategory.Name = fields[0].Value;
                    db.TopCategories.Update(SelectedTopCategory);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void RemoveTopCategory()
        {
            List<Guid> unusedTopCategoriesIds = MarketDbContext.GetUnusedTopCategoriesIds();
            if (unusedTopCategoriesIds.Contains(SelectedTopCategory.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>("", "\"" + SelectedTopCategory.Name + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.TopCategories.Remove(SelectedTopCategory);
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

        private async void RemoveUnusedTopCategories()
        {
            List<TopCategory> unusedTopCategories = MarketDbContext.GetUnusedTopCategories();
            if (unusedTopCategories.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedTopCategories.Select(vt => new Tuple<string, string>("", "\"" + vt.Name + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.TopCategories.RemoveRange(unusedTopCategories);
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



        public async Task QueryDb(bool UpdateMidCategoriesToMatch = true, bool UpdateTopCategories = true)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (UpdateMidCategoriesToMatch)
                {
                    MidCategories = new ObservableCollection<MidCategory>(await db.MidCategories
                        .Where(pcm => ShowUncheckedOnly ? pcm.TopCategoryId == null : true)
                        .Where(pcm => SearchMidCategoriesText == null ? true : pcm.Name.Contains(SearchMidCategoriesText))
                        .AsNoTracking()
                        .ToListAsync()
                         );
                    UncheckedCount = await db.MidCategories.Where(mvt => mvt.TopCategoryId == null).CountAsync();
                }

                if (UpdateTopCategories)
                {
                    TopCategories = new ObservableCollection<TopCategory>(await db.TopCategories
                        .Where(vt => SearchTopCategoriesText == null ? true : vt.Name.Contains(SearchTopCategoriesText))
                        .AsNoTracking()
                        .ToListAsync()
                        );
                }
            }
        }

        public CommandType RemoveMidCategoryCommand { get; }
        public CommandType RemoveUnusedMidCategoriesCommand { get; }
        public CommandType SearchMidCategoriesCommand { get; }
        public CommandType CancelSearchMidCategoriesCommand { get; }

        public CommandType AddTopCategoryCommand { get; }
        public CommandType EditTopCategoryCommand { get; }
        public CommandType RemoveTopCategoryCommand { get; }
        public CommandType RemoveUnusedTopCategoriesCommand { get; }
        public CommandType SearchTopCategoriesCommand { get; }
        public CommandType CancelSearchTopCategoriesCommand { get; }

        public CommandType AddNewTopCategoryBasedOnMatchCommand { get; }
        public CommandType MatchMidCategoriesCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }
        public CommandType ShowNextPageCommand { get; }

        public TopCategoriesPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchTopCategoriesText = "";
            SearchMidCategoriesText = "";

            RemoveMidCategoryCommand = new CommandType();
            RemoveMidCategoryCommand.Create(_ => RemoveMidCategory(), _ => SelectedMidCategory != null);
            RemoveUnusedMidCategoriesCommand = new CommandType();
            RemoveUnusedMidCategoriesCommand.Create(_ => RemoveUnusedMidCategories());
            SearchMidCategoriesCommand = new CommandType();
            SearchMidCategoriesCommand.Create(_ => _ = QueryDb(true, false));
            CancelSearchMidCategoriesCommand = new CommandType();
            CancelSearchMidCategoriesCommand.Create(_ => { SearchMidCategoriesText = ""; _ = QueryDb(true, false); }, _ => SearchMidCategoriesText != "");

            AddTopCategoryCommand = new CommandType();
            AddTopCategoryCommand.Create(_ => AddTopCategory());
            EditTopCategoryCommand = new CommandType();
            EditTopCategoryCommand.Create(_ => EditTopCategory(), _ => SelectedTopCategory != null);
            RemoveTopCategoryCommand = new CommandType();
            RemoveTopCategoryCommand.Create(_ => RemoveTopCategory(), _ => SelectedTopCategory != null);
            RemoveUnusedTopCategoriesCommand = new CommandType();
            RemoveUnusedTopCategoriesCommand.Create(_ => RemoveUnusedTopCategories());
            SearchTopCategoriesCommand = new CommandType();
            SearchTopCategoriesCommand.Create(_ => _ = QueryDb(false, true));
            CancelSearchTopCategoriesCommand = new CommandType();
            CancelSearchTopCategoriesCommand.Create(_ => { SearchTopCategoriesText = ""; _ = QueryDb(false, true); }, _ => SearchTopCategoriesText != "");

            MatchMidCategoriesCommand = new CommandType();
            MatchMidCategoriesCommand.Create(_ => MatchMidCategories(), _ => SelectedTopCategory != null && SelectedMidCategory != null);

            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowMidCategoriesPage());
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowOffersPage());

            _ = QueryDb(false, true); //Query with true,false executes when ShowUncheckedOnly property is set
        }

    }
}
