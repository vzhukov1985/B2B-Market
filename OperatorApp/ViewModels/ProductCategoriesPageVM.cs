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
    public class ProductCategoriesPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ObservableCollection<MatchProductCategory> _productCategoriesToMatch;
        public ObservableCollection<MatchProductCategory> ProductCategoriesToMatch
        {
            get { return _productCategoriesToMatch; }
            set
            {
                _productCategoriesToMatch = value;
                OnPropertyChanged("ProductCategoriesToMatch");
            }
        }

        private ObservableCollection<ProductCategory> _productCategories;
        public ObservableCollection<ProductCategory> ProductCategories
        {
            get { return _productCategories; }
            set
            {
                _productCategories = value;
                OnPropertyChanged("ProductCategories");
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

        private MatchProductCategory _selectedMatchProductCategory;
        public MatchProductCategory SelectedMatchProductCategory
        {
            get { return _selectedMatchProductCategory; }
            set
            {
                _selectedMatchProductCategory = value;

                if (ProductCategories != null && _selectedMatchProductCategory != null && _selectedMatchProductCategory.ProductCategoryId != null)
                    SelectedProductCategory = ProductCategories.Where(vt => vt.Id == _selectedMatchProductCategory.ProductCategoryId).FirstOrDefault();

                OnPropertyChanged("SelectedMatchProductCategory");
            }
        }

        private ProductCategory _selectedProductCategory;
        public ProductCategory SelectedProductCategory
        {
            get { return _selectedProductCategory; }
            set
            {
                _selectedProductCategory = value;
                OnPropertyChanged("SelectedProductCategory");
            }
        }

        private string _searchMatchProductCategoriesText;
        public string SearchMatchProductCategoriesText
        {
            get { return _searchMatchProductCategoriesText; }
            set
            {
                _searchMatchProductCategoriesText = value;
                OnPropertyChanged("SearchMatchProductCategoriesText");
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

        private async void RemoveMatchProductCategory()
        {
            List<Guid> unusedProductCategoriesIds = MarketDbContext.GetUnusedMatchProductCategoriesIds();
            if (unusedProductCategoriesIds.Contains(SelectedMatchProductCategory.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>(SelectedMatchProductCategory.Supplier.ShortName, "\"" + SelectedMatchProductCategory.SupplierProductCategoryName + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchProductCategories.Remove(SelectedMatchProductCategory);
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

        private async void RemoveUnusedMatchProductCategories()
        {
            List<MatchProductCategory> unusedProductCategories = MarketDbContext.GetUnusedMatchProductCategories();
            if (unusedProductCategories.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedProductCategories.Select(vt => new Tuple<string, string>(vt.Supplier.ShortName, "\"" + vt.SupplierProductCategoryName + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchProductCategories.RemoveRange(unusedProductCategories);
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

        private async void AddNewProductCategoryBasedOnMatch()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                ProductCategory productCategory = db.ProductCategories.Where(vt => vt.Name == SelectedMatchProductCategory.SupplierProductCategoryName).FirstOrDefault();
                if (productCategory == null)
                {
                    productCategory = new ProductCategory { Id = Guid.NewGuid(), Name = SelectedMatchProductCategory.SupplierProductCategoryName };
                    db.ProductCategories.Add(productCategory);
                    await db.SaveChangesAsync();
                    SelectedMatchProductCategory.ProductCategoryId = productCategory.Id;
                    db.MatchProductCategories.Update(SelectedMatchProductCategory);
                    await db.SaveChangesAsync();
                    _ = QueryDb();
                }
                else
                {
                    DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Проверьте и свяжите с ней или другой позицией", "Ошибка");
                    if (ProductCategories.Where(vt => vt.Name == SelectedMatchProductCategory.SupplierProductCategoryName).FirstOrDefault() == null)
                    {
                        SearchProductCategoriesText = "";
                    }
                    await QueryDb(false, true);
                    SelectedProductCategory = ProductCategories.Where(vt => vt.Name == SelectedMatchProductCategory.SupplierProductCategoryName).FirstOrDefault();
                }
            }
        }

        private async void MatchProductCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                SelectedMatchProductCategory.ProductCategoryId = SelectedProductCategory.Id;
                db.Update(SelectedMatchProductCategory);
                await db.SaveChangesAsync();
                _ = QueryDb(true, false);
            }
        }

        private async void AddProductCategory()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", "")
            };

            fields = DialogService.ShowAddEditElementDlg(fields, false);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    ProductCategory newProductCategory = new ProductCategory { Id = Guid.NewGuid(), Name = fields[0].Value };
                    if (db.ProductCategories.Where(vt => vt.Name == newProductCategory.Name).FirstOrDefault() != null)
                    {
                        DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                        if (ProductCategories.Where(vt => vt.Name == newProductCategory.Name).FirstOrDefault() == null)
                        {
                            SearchProductCategoriesText = "";
                        }
                        await QueryDb(false, true);
                        SelectedProductCategory = ProductCategories.Where(vt => vt.Name == newProductCategory.Name).FirstOrDefault();
                        return;
                    }

                    db.ProductCategories.Add(newProductCategory);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void EditProductCategory()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", SelectedProductCategory.Name)
            };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedProductCategory.Name = fields[0].Value;
                    db.ProductCategories.Update(SelectedProductCategory);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void RemoveProductCategory()
        {
            List<Guid> unusedProductCategoriesIds = MarketDbContext.GetUnusedProductCategoriesIds();
            if (unusedProductCategoriesIds.Contains(SelectedProductCategory.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>("", "\"" + SelectedProductCategory.Name + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.ProductCategories.Remove(SelectedProductCategory);
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

        private async void RemoveUnusedProductCategories()
        {
            List<ProductCategory> unusedProductCategories = MarketDbContext.GetUnusedProductCategories();
            if (unusedProductCategories.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedProductCategories.Select(vt => new Tuple<string, string>("", "\"" + vt.Name + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.ProductCategories.RemoveRange(unusedProductCategories);
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



        public async Task QueryDb(bool UpdateProductCategoriesToMatch = true, bool UpdateProductCategories = true)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (UpdateProductCategoriesToMatch)
                {
                    ProductCategoriesToMatch = new ObservableCollection<MatchProductCategory>(await db.MatchProductCategories
                        .Include(mvt => mvt.Supplier)
                        .Where(mvt => ShowUncheckedOnly ? mvt.ProductCategoryId == null : true)
                        .Where(mvt => SearchMatchProductCategoriesText == null ? true :
                            EF.Functions.Like(mvt.SupplierProductCategoryName, $"%{SearchMatchProductCategoriesText}%") ||
                            EF.Functions.Like(mvt.Supplier.ShortName, $"%{SearchMatchProductCategoriesText}%"))
                        .AsNoTracking()
                        .ToListAsync()
                         );
                    UncheckedCount = await db.MatchProductCategories.Where(mvt => mvt.ProductCategoryId == null).CountAsync();
                }

                if (UpdateProductCategories)
                {
                    ProductCategories = new ObservableCollection<ProductCategory>(await db.ProductCategories
                        .Where(vt => SearchProductCategoriesText == null ? true : EF.Functions.Like(vt.Name, $"%{SearchProductCategoriesText}%"))
                        .AsNoTracking()
                        .ToListAsync()
                        );
                }
            }
        }

        public CommandType RemoveMatchProductCategoryCommand { get; }
        public CommandType RemoveUnusedMatchProductCategoriesCommand { get; }
        public CommandType SearchMatchProductCategoriesCommand { get; }
        public CommandType CancelSearchMatchProductCategoriesCommand { get; }

        public CommandType AddProductCategoryCommand { get; }
        public CommandType EditProductCategoryCommand { get; }
        public CommandType RemoveProductCategoryCommand { get; }
        public CommandType RemoveUnusedProductCategoriesCommand { get; }
        public CommandType SearchProductCategoriesCommand { get; }
        public CommandType CancelSearchProductCategoriesCommand { get; }

        public CommandType AddNewProductCategoryBasedOnMatchCommand { get; }
        public CommandType MatchProductCategoriesCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }
        public CommandType ShowNextPageCommand { get; }

        public ProductCategoriesPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchProductCategoriesText = "";
            SearchMatchProductCategoriesText = "";

            RemoveMatchProductCategoryCommand = new CommandType();
            RemoveMatchProductCategoryCommand.Create(_ => RemoveMatchProductCategory(), _ => SelectedMatchProductCategory != null);
            RemoveUnusedMatchProductCategoriesCommand = new CommandType();
            RemoveUnusedMatchProductCategoriesCommand.Create(_ => RemoveUnusedMatchProductCategories());
            SearchMatchProductCategoriesCommand = new CommandType();
            SearchMatchProductCategoriesCommand.Create(_ => _ = QueryDb(true, false));
            CancelSearchMatchProductCategoriesCommand = new CommandType();
            CancelSearchMatchProductCategoriesCommand.Create(_ => { SearchMatchProductCategoriesText = ""; _ = QueryDb(true, false); }, _ => SearchMatchProductCategoriesText != "");

            AddProductCategoryCommand = new CommandType();
            AddProductCategoryCommand.Create(_ => AddProductCategory());
            EditProductCategoryCommand = new CommandType();
            EditProductCategoryCommand.Create(_ => EditProductCategory(), _ => SelectedProductCategory != null);
            RemoveProductCategoryCommand = new CommandType();
            RemoveProductCategoryCommand.Create(_ => RemoveProductCategory(), _ => SelectedProductCategory != null);
            RemoveUnusedProductCategoriesCommand = new CommandType();
            RemoveUnusedProductCategoriesCommand.Create(_ => RemoveUnusedProductCategories());
            SearchProductCategoriesCommand = new CommandType();
            SearchProductCategoriesCommand.Create(_ => _ = QueryDb(false, true));
            CancelSearchProductCategoriesCommand = new CommandType();
            CancelSearchProductCategoriesCommand.Create(_ => { SearchProductCategoriesText = ""; _ = QueryDb(false, true); }, _ => SearchProductCategoriesText != "");

            AddNewProductCategoryBasedOnMatchCommand = new CommandType();
            AddNewProductCategoryBasedOnMatchCommand.Create(_ => AddNewProductCategoryBasedOnMatch(), _ => SelectedMatchProductCategory != null);
            MatchProductCategoriesCommand = new CommandType();
            MatchProductCategoriesCommand.Create(_ => MatchProductCategories(), _ => SelectedProductCategory != null && SelectedMatchProductCategory != null);

            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowProductExtraPropertyTypesPage());
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowMidCategoriesPage());

            _ = QueryDb(false, true); //Query with true,false executes when ShowUncheckedOnly property is set
        }

    }
}
