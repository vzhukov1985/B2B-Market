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
    public class ProductExtraPropertyTypesPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ObservableCollection<MatchProductExtraPropertyType> _productExtraPropertyTypesToMatch;
        public ObservableCollection<MatchProductExtraPropertyType> ProductExtraPropertyTypesToMatch
        {
            get { return _productExtraPropertyTypesToMatch; }
            set
            {
                _productExtraPropertyTypesToMatch = value;
                OnPropertyChanged("ProductExtraPropertyTypesToMatch");
            }
        }

        private ObservableCollection<ProductExtraPropertyType> _productExtraPropertyTypes;
        public ObservableCollection<ProductExtraPropertyType> ProductExtraPropertyTypes
        {
            get { return _productExtraPropertyTypes; }
            set
            {
                _productExtraPropertyTypes = value;
                OnPropertyChanged("ProductExtraPropertyTypes");
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

        private MatchProductExtraPropertyType _selectedMatchProductExtraPropertyType;
        public MatchProductExtraPropertyType SelectedMatchProductExtraPropertyType
        {
            get { return _selectedMatchProductExtraPropertyType; }
            set
            {
                _selectedMatchProductExtraPropertyType = value;

                if (ProductExtraPropertyTypes != null && _selectedMatchProductExtraPropertyType != null && _selectedMatchProductExtraPropertyType.ProductExtraPropertyTypeId != null)
                    SelectedProductExtraPropertyType = ProductExtraPropertyTypes.Where(pept => pept.Id == _selectedMatchProductExtraPropertyType.ProductExtraPropertyTypeId).FirstOrDefault();

                OnPropertyChanged("SelectedMatchProductExtraPropertyType");
            }
        }

        private ProductExtraPropertyType _selectedProductExtraPropertyType;
        public ProductExtraPropertyType SelectedProductExtraPropertyType
        {
            get { return _selectedProductExtraPropertyType; }
            set
            {
                _selectedProductExtraPropertyType = value;
                OnPropertyChanged("SelectedProductExtraPropertyType");
            }
        }

        private string _searchMatchProductExtraPropertyTypesText;
        public string SearchMatchProductExtraPropertyTypesText
        {
            get { return _searchMatchProductExtraPropertyTypesText; }
            set
            {
                _searchMatchProductExtraPropertyTypesText = value;
                OnPropertyChanged("SearchMatchProductExtraPropertyTypesText");
            }
        }

        private string _searchProductExtraPropertyTypesText;
        public string SearchProductExtraPropertyTypesText
        {
            get { return _searchProductExtraPropertyTypesText; }
            set
            {
                _searchProductExtraPropertyTypesText = value;
                OnPropertyChanged("SearchProductExtraPropertyTypesText");
            }
        }

        private async void RemoveMatchProductExtraPropertyType()
        {
            List<Guid> unusedProductExtraPropertyTypesIds = MarketDbContext.GetUnusedMatchProductExtraPropertyTypesIds();
            if (unusedProductExtraPropertyTypesIds.Contains(SelectedMatchProductExtraPropertyType.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>(SelectedMatchProductExtraPropertyType.Supplier.ShortName, "\"" + SelectedMatchProductExtraPropertyType.SupplierProductExtraPropertyTypeName + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchProductExtraPropertyTypes.Remove(SelectedMatchProductExtraPropertyType);
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

        private async void RemoveUnusedMatchProductExtraPropertyTypes()
        {
            List<MatchProductExtraPropertyType> unusedProductExtraPropertyTypes = MarketDbContext.GetUnusedMatchProductExtraPropertyTypes();
            if (unusedProductExtraPropertyTypes.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedProductExtraPropertyTypes.Select(pept => new Tuple<string, string>(pept.Supplier.ShortName, "\"" + pept.SupplierProductExtraPropertyTypeName + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.MatchProductExtraPropertyTypes.RemoveRange(unusedProductExtraPropertyTypes);
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

        private async void AddNewProductExtraPropertyTypeBasedOnMatch()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                ProductExtraPropertyType productExtraPropertyType = db.ProductExtraPropertyTypes.Where(pept => pept.Name == SelectedMatchProductExtraPropertyType.SupplierProductExtraPropertyTypeName).FirstOrDefault();
                if (productExtraPropertyType == null)
                {
                    productExtraPropertyType = new ProductExtraPropertyType { Id = Guid.NewGuid(), Name = SelectedMatchProductExtraPropertyType.SupplierProductExtraPropertyTypeName };
                    db.ProductExtraPropertyTypes.Add(productExtraPropertyType);
                    await db.SaveChangesAsync();
                    SelectedMatchProductExtraPropertyType.ProductExtraPropertyTypeId = productExtraPropertyType.Id;
                    db.MatchProductExtraPropertyTypes.Update(SelectedMatchProductExtraPropertyType);
                    await db.SaveChangesAsync();
                    _ = QueryDb();
                }
                else
                {
                    DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Проверьте и свяжите с ней или другой позицией", "Ошибка");
                    if (ProductExtraPropertyTypes.Where(pept => pept.Name == SelectedMatchProductExtraPropertyType.SupplierProductExtraPropertyTypeName).FirstOrDefault() == null)
                    {
                        SearchProductExtraPropertyTypesText = "";
                    }
                    await QueryDb(false, true);
                    SelectedProductExtraPropertyType = ProductExtraPropertyTypes.Where(pept => pept.Name == SelectedMatchProductExtraPropertyType.SupplierProductExtraPropertyTypeName).FirstOrDefault();
                }
            }
        }

        private async void MatchProductExtraPropertyTypes()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                SelectedMatchProductExtraPropertyType.ProductExtraPropertyTypeId = SelectedProductExtraPropertyType.Id;
                db.Update(SelectedMatchProductExtraPropertyType);
                await db.SaveChangesAsync();
                _ = QueryDb(true, false);
            }
        }

        private async void AddProductExtraPropertyType()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", "")
            };

            fields = DialogService.ShowAddEditElementDlg(fields, false);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    ProductExtraPropertyType newProductExtraPropertyType = new ProductExtraPropertyType { Id = Guid.NewGuid(), Name = fields[0].Value };
                    if (db.ProductExtraPropertyTypes.Where(pept => pept.Name == newProductExtraPropertyType.Name).FirstOrDefault() != null)
                    {
                        DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                        if (ProductExtraPropertyTypes.Where(pept => pept.Name == newProductExtraPropertyType.Name).FirstOrDefault() == null)
                        {
                            SearchProductExtraPropertyTypesText = "";
                        }
                        await QueryDb(false, true);
                        SelectedProductExtraPropertyType = ProductExtraPropertyTypes.Where(pept => pept.Name == newProductExtraPropertyType.Name).FirstOrDefault();
                        return;
                    }

                    db.ProductExtraPropertyTypes.Add(newProductExtraPropertyType);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void EditProductExtraPropertyType()
        {
            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", SelectedProductExtraPropertyType.Name)
            };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedProductExtraPropertyType.Name = fields[0].Value;
                    db.ProductExtraPropertyTypes.Update(SelectedProductExtraPropertyType);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }
        }

        private async void RemoveProductExtraPropertyType()
        {
            List<Guid> unusedProductExtraPropertyTypesIds = MarketDbContext.GetUnusedProductExtraPropertyTypesIds();
            if (unusedProductExtraPropertyTypesIds.Contains(SelectedProductExtraPropertyType.Id))
            {
                Tuple<string, string> element = new Tuple<string, string>("", "\"" + SelectedProductExtraPropertyType.Name + "\"");
                if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.ProductExtraPropertyTypes.Remove(SelectedProductExtraPropertyType);
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

        private async void RemoveUnusedProductExtraPropertyTypes()
        {
            List<ProductExtraPropertyType> unusedProductExtraPropertyTypes = MarketDbContext.GetUnusedProductExtraPropertyTypes();
            if (unusedProductExtraPropertyTypes.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedProductExtraPropertyTypes.Select(pept => new Tuple<string, string>("", "\"" + pept.Name + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.ProductExtraPropertyTypes.RemoveRange(unusedProductExtraPropertyTypes);
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



        public async Task QueryDb(bool UpdateProductExtraPropertyTypesToMatch = true, bool UpdateProductExtraPropertyTypes = true)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (UpdateProductExtraPropertyTypesToMatch)
                {
                    ProductExtraPropertyTypesToMatch = new ObservableCollection<MatchProductExtraPropertyType>(await db.MatchProductExtraPropertyTypes
                        .Include(mpept => mpept.Supplier)
                        .Where(mpept => ShowUncheckedOnly ? mpept.ProductExtraPropertyTypeId == null : true)
                        .Where(mpept => SearchMatchProductExtraPropertyTypesText == null ? true : mpept.SupplierProductExtraPropertyTypeName.Contains(SearchMatchProductExtraPropertyTypesText) || mpept.Supplier.ShortName.Contains(SearchMatchProductExtraPropertyTypesText))
                        .AsNoTracking()
                        .ToListAsync()
                         );
                    UncheckedCount = await db.MatchProductExtraPropertyTypes.Where(mpept => mpept.ProductExtraPropertyTypeId == null).CountAsync();
                }

                if (UpdateProductExtraPropertyTypes)
                {
                    ProductExtraPropertyTypes = new ObservableCollection<ProductExtraPropertyType>(await db.ProductExtraPropertyTypes
                        .Where(pept => SearchProductExtraPropertyTypesText == null ? true : pept.Name.Contains(SearchProductExtraPropertyTypesText))
                        .AsNoTracking()
                        .ToListAsync()
                        );
                }
            }
        }

        public CommandType RemoveMatchProductExtraPropertyTypeCommand { get; }
        public CommandType RemoveUnusedMatchProductExtraPropertyTypesCommand { get; }
        public CommandType SearchMatchProductExtraPropertyTypesCommand { get; }
        public CommandType CancelSearchMatchProductExtraPropertyTypesCommand { get; }

        public CommandType AddProductExtraPropertyTypeCommand { get; }
        public CommandType EditProductExtraPropertyTypeCommand { get; }
        public CommandType RemoveProductExtraPropertyTypeCommand { get; }
        public CommandType RemoveUnusedProductExtraPropertyTypesCommand { get; }
        public CommandType SearchProductExtraPropertyTypesCommand { get; }
        public CommandType CancelSearchProductExtraPropertyTypesCommand { get; }

        public CommandType AddNewProductExtraPropertyTypeBasedOnMatchCommand { get; }
        public CommandType MatchProductExtraPropertyTypesCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }
        public CommandType ShowNextPageCommand { get; }

        public ProductExtraPropertyTypesPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchProductExtraPropertyTypesText = "";
            SearchMatchProductExtraPropertyTypesText = "";

            RemoveMatchProductExtraPropertyTypeCommand = new CommandType();
            RemoveMatchProductExtraPropertyTypeCommand.Create(_ => RemoveMatchProductExtraPropertyType(), _ => SelectedMatchProductExtraPropertyType != null);
            RemoveUnusedMatchProductExtraPropertyTypesCommand = new CommandType();
            RemoveUnusedMatchProductExtraPropertyTypesCommand.Create(_ => RemoveUnusedMatchProductExtraPropertyTypes());
            SearchMatchProductExtraPropertyTypesCommand = new CommandType();
            SearchMatchProductExtraPropertyTypesCommand.Create(_ => _ = QueryDb(true, false));
            CancelSearchMatchProductExtraPropertyTypesCommand = new CommandType();
            CancelSearchMatchProductExtraPropertyTypesCommand.Create(_ => { SearchMatchProductExtraPropertyTypesText = ""; _ = QueryDb(true, false); }, _ => SearchMatchProductExtraPropertyTypesText != "");

            AddProductExtraPropertyTypeCommand = new CommandType();
            AddProductExtraPropertyTypeCommand.Create(_ => AddProductExtraPropertyType());
            EditProductExtraPropertyTypeCommand = new CommandType();
            EditProductExtraPropertyTypeCommand.Create(_ => EditProductExtraPropertyType(), _ => SelectedProductExtraPropertyType != null);
            RemoveProductExtraPropertyTypeCommand = new CommandType();
            RemoveProductExtraPropertyTypeCommand.Create(_ => RemoveProductExtraPropertyType(), _ => SelectedProductExtraPropertyType != null);
            RemoveUnusedProductExtraPropertyTypesCommand = new CommandType();
            RemoveUnusedProductExtraPropertyTypesCommand.Create(_ => RemoveUnusedProductExtraPropertyTypes());
            SearchProductExtraPropertyTypesCommand = new CommandType();
            SearchProductExtraPropertyTypesCommand.Create(_ => _ = QueryDb(false, true));
            CancelSearchProductExtraPropertyTypesCommand = new CommandType();
            CancelSearchProductExtraPropertyTypesCommand.Create(_ => { SearchProductExtraPropertyTypesText = ""; _ = QueryDb(false, true); }, _ => SearchProductExtraPropertyTypesText != "");

            AddNewProductExtraPropertyTypeBasedOnMatchCommand = new CommandType();
            AddNewProductExtraPropertyTypeBasedOnMatchCommand.Create(_ => AddNewProductExtraPropertyTypeBasedOnMatch(), _ => SelectedMatchProductExtraPropertyType != null);
            MatchProductExtraPropertyTypesCommand = new CommandType();
            MatchProductExtraPropertyTypesCommand.Create(_ => MatchProductExtraPropertyTypes(), _ => SelectedProductExtraPropertyType != null && SelectedMatchProductExtraPropertyType != null);

            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowVolumeUnitsPage());
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowProductCategoriesPage());

            _ = QueryDb(false, true); //Query with true,false executes when ShowUncheckedOnly property is set
        }

    }
}
