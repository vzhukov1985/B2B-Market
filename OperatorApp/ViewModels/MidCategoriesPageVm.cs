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
    public class MidCategoriesPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ObservableCollection<ProductCategory> _productCategoriesToMatch;
        public ObservableCollection<ProductCategory> ProductCategories
        {
            get { return _productCategoriesToMatch; }
            set
            {
                _productCategoriesToMatch = value;
                OnPropertyChanged("ProductCategories");
            }
        }

        private ObservableCollection<MidCategory> _midCategories;
        public ObservableCollection<MidCategory> MidCategories
        {
            get { return _midCategories; }
            set
            {
                _midCategories = value;
                OnPropertyChanged("MidCategories");
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

        private ProductCategory _selectedProductCategory;
        public ProductCategory SelectedProductCategory
        {
            get { return _selectedProductCategory; }
            set
            {
                _selectedProductCategory = value;

                if (MidCategories != null && _selectedProductCategory != null && _selectedProductCategory.MidCategoryId != null)
                    SelectedMidCategory = MidCategories.Where(vt => vt.Id == _selectedProductCategory.MidCategoryId).FirstOrDefault();

                OnPropertyChanged("SelectedProductCategory");
            }
        }

        private MidCategory _selectedMidCategory;
        public MidCategory SelectedMidCategory
        {
            get { return _selectedMidCategory; }
            set
            {
                _selectedMidCategory = value;
                OnPropertyChanged("SelectedMidCategory");
            }
        }

        private string _searchProductCategoriesText;
        public string SearchProductCategoriesText
        {
            get { return _searchProductCategoriesText; }
            set
            {
                _searchProductCategoriesText = value;
                OnPropertyChanged("SearchProductCategoriesText");
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

        private async void RemoveProductCategory()
        {
            List<Guid> unusedCategoriesIds = MarketDbContext.GetUnusedProductCategoriesIds();
            if (unusedCategoriesIds.Contains(SelectedProductCategory.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>("", "\"" + SelectedProductCategory.Name + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.ProductCategories.Remove(SelectedProductCategory);
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

        private async void RemoveUnusedProductCategories()
        {
            List<ProductCategory> unusedCategories = MarketDbContext.GetUnusedProductCategories();
            if (unusedCategories.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedCategories.Select(vt => new Tuple<string, string>("", "\"" + vt.Name + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.ProductCategories.RemoveRange(unusedCategories);
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

        private async void MatchProductCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                SelectedProductCategory.MidCategoryId = SelectedMidCategory.Id;
                db.Update(SelectedProductCategory);
                await db.SaveChangesAsync();
                _ = QueryDb(true, false);
            }
        }

        private async void AddMidCategory()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", "")
            };

            fields = DialogService.ShowAddEditElementDlg(fields, false);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    MidCategory newMidCategory = new MidCategory { Id = Guid.NewGuid(), Name = fields[0].Value };
                    if (db.MidCategories.Where(vt => vt.Name == newMidCategory.Name).FirstOrDefault() != null)
                    {
                        DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                        if (MidCategories.Where(vt => vt.Name == newMidCategory.Name).FirstOrDefault() == null)
                        {
                            SearchMidCategoriesText = "";
                        }
                        await QueryDb(false, true);
                        SelectedMidCategory = MidCategories.Where(vt => vt.Name == newMidCategory.Name).FirstOrDefault();
                        return;
                    }

                    db.MidCategories.Add(newMidCategory);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void EditMidCategory()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", SelectedMidCategory.Name)
            };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedMidCategory.Name = fields[0].Value;
                    db.MidCategories.Update(SelectedMidCategory);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void RemoveMidCategory()
        {
            List<Guid> unusedMidCategoriesIds = MarketDbContext.GetUnusedMidCategoriesIds();
            if (unusedMidCategoriesIds.Contains(SelectedMidCategory.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>("", "\"" + SelectedMidCategory.Name + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MidCategories.Remove(SelectedMidCategory);
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

        private async void RemoveUnusedMidCategories()
        {
            List<MidCategory> unusedMidCategories = MarketDbContext.GetUnusedMidCategories();
            if (unusedMidCategories.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedMidCategories.Select(vt => new Tuple<string, string>("", "\"" + vt.Name + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MidCategories.RemoveRange(unusedMidCategories);
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



        public async Task QueryDb(bool UpdateProductCategoriesToMatch = true, bool UpdateMidCategories = true)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (UpdateProductCategoriesToMatch)
                {
                    ProductCategories = new ObservableCollection<ProductCategory>(await db.ProductCategories
                        .Where(pcm => ShowUncheckedOnly ? pcm.MidCategoryId == null : true)
                        .Where(pcm => SearchProductCategoriesText == null ? true : EF.Functions.Like(pcm.Name, $"%{SearchProductCategoriesText}%"))
                        .AsNoTracking()
                        .ToListAsync()
                         );
                    UncheckedCount = await db.ProductCategories.Where(mvt => mvt.MidCategoryId == null).CountAsync();
                }

                if (UpdateMidCategories)
                {
                    MidCategories = new ObservableCollection<MidCategory>(await db.MidCategories
                        .Where(vt => SearchMidCategoriesText == null ? true : EF.Functions.Like(vt.Name, $"%{SearchMidCategoriesText}%"))
                        .AsNoTracking()
                        .ToListAsync()
                        );
                }
            }
        }

        public CommandType RemoveProductCategoryCommand { get; }
        public CommandType RemoveUnusedProductCategoriesCommand { get; }
        public CommandType SearchProductCategoriesCommand { get; }
        public CommandType CancelSearchProductCategoriesCommand { get; }

        public CommandType AddMidCategoryCommand { get; }
        public CommandType EditMidCategoryCommand { get; }
        public CommandType RemoveMidCategoryCommand { get; }
        public CommandType RemoveUnusedMidCategoriesCommand { get; }
        public CommandType SearchMidCategoriesCommand { get; }
        public CommandType CancelSearchMidCategoriesCommand { get; }

        public CommandType AddNewMidCategoryBasedOnMatchCommand { get; }
        public CommandType MatchProductCategoriesCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }
        public CommandType ShowNextPageCommand { get; }

        public MidCategoriesPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchMidCategoriesText = "";
            SearchProductCategoriesText = "";

            RemoveProductCategoryCommand = new CommandType();
            RemoveProductCategoryCommand.Create(_ => RemoveProductCategory(), _ => SelectedProductCategory != null);
            RemoveUnusedProductCategoriesCommand = new CommandType();
            RemoveUnusedProductCategoriesCommand.Create(_ => RemoveUnusedProductCategories());
            SearchProductCategoriesCommand = new CommandType();
            SearchProductCategoriesCommand.Create(_ => _ = QueryDb(true, false));
            CancelSearchProductCategoriesCommand = new CommandType();
            CancelSearchProductCategoriesCommand.Create(_ => { SearchProductCategoriesText = ""; _ = QueryDb(true, false); }, _ => SearchProductCategoriesText != "");

            AddMidCategoryCommand = new CommandType();
            AddMidCategoryCommand.Create(_ => AddMidCategory());
            EditMidCategoryCommand = new CommandType();
            EditMidCategoryCommand.Create(_ => EditMidCategory(), _ => SelectedMidCategory != null);
            RemoveMidCategoryCommand = new CommandType();
            RemoveMidCategoryCommand.Create(_ => RemoveMidCategory(), _ => SelectedMidCategory != null);
            RemoveUnusedMidCategoriesCommand = new CommandType();
            RemoveUnusedMidCategoriesCommand.Create(_ => RemoveUnusedMidCategories());
            SearchMidCategoriesCommand = new CommandType();
            SearchMidCategoriesCommand.Create(_ => _ = QueryDb(false, true));
            CancelSearchMidCategoriesCommand = new CommandType();
            CancelSearchMidCategoriesCommand.Create(_ => { SearchMidCategoriesText = ""; _ = QueryDb(false, true); }, _ => SearchMidCategoriesText != "");

            MatchProductCategoriesCommand = new CommandType();
            MatchProductCategoriesCommand.Create(_ => MatchProductCategories(), _ => SelectedMidCategory != null && SelectedProductCategory != null);

            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowProductCategoriesPage());
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowTopCategoriesPage());

            _ = QueryDb(false, true); //Query with true,false executes when ShowUncheckedOnly property is set
        }

    }
}
