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
using TextCopy;

namespace OperatorApp.ViewModels
{
    public class CategoriesPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private int _sortType;
        public int SortType
        {
            get { return _sortType; }
            set
            {
                _sortType = value;
                if (allProductCategoriesToMatchCategories != null)
                {
                    if (value == 0)
                    {
                        ProductCategoriesToMatch = new ObservableCollection<MatchProductCategory>(allProductCategoriesToMatchCategories
                                                    .Where(mpc => ShowUncheckedOnly == false ? true : mpc.ProductCategoryId == null)
                                                    .OrderBy(mpc => mpc.SupplierProductCategoryName));
                    }
                    else
                    {
                        ProductCategoriesToMatch = new ObservableCollection<MatchProductCategory>(allProductCategoriesToMatchCategories
                                                    .Where(mpc => ShowUncheckedOnly == false ? true : mpc.ProductCategoryId == null)
                                                    .OrderBy(mpc => mpc.Supplier.ShortName)
                                                    .ThenBy(mpc => mpc.SupplierProductCategoryName));
                    }
                }
                OnPropertyChanged("SortType");
            }
        }


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

                if (allProductCategoriesToMatchCategories != null)
                {
                    if (SortType == 0)
                    {
                        ProductCategoriesToMatch = new ObservableCollection<MatchProductCategory>(allProductCategoriesToMatchCategories
                                                    .Where(mpc => value == false ? true : mpc.ProductCategoryId == null)
                                                    .OrderBy(mpc => mpc.SupplierProductCategoryName));
                    }
                    else
                    {
                        ProductCategoriesToMatch = new ObservableCollection<MatchProductCategory>(allProductCategoriesToMatchCategories
                                                    .Where(mpc => value == false ? true : mpc.ProductCategoryId == null)
                                                    .OrderBy(mpc => mpc.Supplier.ShortName)
                                                    .ThenBy(mpc => mpc.SupplierProductCategoryName));
                    }
                }
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

                if (value != null && value.ProductCategoryId != null)
                {
                    _selectedMatchProductCategory.ProductCategory = allProductCategories.Where(pc => pc.Id == _selectedMatchProductCategory.ProductCategoryId).FirstOrDefault();
                    _selectedMatchProductCategory.ProductCategory.MidCategory = allMidCategories.Where(mc => mc.Id == _selectedMatchProductCategory.ProductCategory.MidCategoryId).FirstOrDefault();
                    _selectedMatchProductCategory.ProductCategory.MidCategory.TopCategory = allTopCategories.Where(tc => tc.Id == _selectedMatchProductCategory.ProductCategory.MidCategory.TopCategoryId).FirstOrDefault();
                    SelectedTopCategory = _selectedMatchProductCategory.ProductCategory.MidCategory.TopCategory;
                    SelectedMidCategory = _selectedMatchProductCategory.ProductCategory.MidCategory;
                    SelectedProductCategory = _selectedMatchProductCategory.ProductCategory;
                    _selectedMatchProductCategory.ProductCategory = null;
                }

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

        private MidCategory _selectedMidCategory;
        public MidCategory SelectedMidCategory
        {
            get { return _selectedMidCategory; }
            set
            {
                _selectedMidCategory = value;
                if (value != null)
                {
                    ProductCategories = new ObservableCollection<ProductCategory>(allProductCategories.Where(pc => pc.MidCategoryId == value.Id).OrderBy(pc => pc.Name));
                    if (ProductCategories.Count > 0) SelectedProductCategory = ProductCategories[0];
                }
                else
                {
                    ProductCategories.Clear();
                }
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
                if (value != null)
                {
                    MidCategories = new ObservableCollection<MidCategory>(allMidCategories.Where(mc => mc.TopCategoryId == value.Id).OrderBy(mc => mc.Name));
                    if (MidCategories.Count > 0) SelectedMidCategory = MidCategories[0];
                }
                else
                {
                    MidCategories.Clear();
                }
                OnPropertyChanged("SelectedTopCategory");
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
                    }
                    allProductCategoriesToMatchCategories.Remove(SelectedMatchProductCategory);
                    ProductCategoriesToMatch.Remove(SelectedMatchProductCategory);
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
                    }
                }
                allProductCategoriesToMatchCategories.RemoveAll(apc => unusedProductCategories.Select(upc => upc.Id).Contains(apc.Id));
                ProductCategoriesToMatch.RemoveAll(pc => unusedProductCategories.Select(upc => upc.Id).Contains(pc.Id));
            }
            else
            {
                DialogService.ShowMessageDialog("Все элементы в списке используются", "Удаление невозможно");
            }
        }

        private void SearchMatchProductCategories()
        {
            if (!string.IsNullOrEmpty(SearchMatchProductCategoriesText))
            {
                var foundMatchProductCategory = ProductCategoriesToMatch.Where(mpc => EF.Functions.Like(mpc.SupplierProductCategoryName, $"%{SearchMatchProductCategoriesText}%") ||
                                                                                      EF.Functions.Like(mpc.Supplier.ShortName, $"%{SearchMatchProductCategoriesText}%")).FirstOrDefault();
                if (foundMatchProductCategory != null) SelectedMatchProductCategory = foundMatchProductCategory;
            }
        }

        private async void AddNewProductCategoryBasedOnMatch()
        {
            List<ElementField> fields = new List<ElementField> { new ElementField("Название", SelectedMatchProductCategory.SupplierProductCategoryName) };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                ProductCategory foundProductCategory = ProductCategories.Where(vt => vt.Name == SelectedMatchProductCategory.SupplierProductCategoryName).FirstOrDefault();
                if (foundProductCategory == null)
                {
                    var newProductCategory = new ProductCategory { Id = Guid.NewGuid(), Name = fields[0].Value, MidCategoryId = SelectedMidCategory.Id };
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.ProductCategories.Add(newProductCategory);
                        allProductCategories.Add(newProductCategory);
                        ProductCategories.Add(newProductCategory);
                        SelectedMatchProductCategory.ProductCategoryId = newProductCategory.Id;
                        db.MatchProductCategories.Update(SelectedMatchProductCategory);
                        await db.SaveChangesAsync();
                        if (ShowUncheckedOnly) ProductCategoriesToMatch.Remove(SelectedMatchProductCategory);
                    }
                }
                else
                {
                    DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Проверьте и свяжите с ней или другой позицией", "Ошибка");
                    SelectedProductCategory = foundProductCategory;
                }

            }
        }

        private async void MatchProductCategories()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                SelectedMatchProductCategory.ProductCategoryId = SelectedProductCategory.Id;
                db.MatchProductCategories.Update(SelectedMatchProductCategory);
                await db.SaveChangesAsync();
                if (ShowUncheckedOnly) ProductCategoriesToMatch.Remove(SelectedMatchProductCategory);
            }
        }

        private async void AddProductCategory()
        {
            List<ElementField> fields = new List<ElementField> { new ElementField("Название", "") };

            fields = DialogService.ShowAddEditElementDlg(fields, false);
            if (fields != null)
            {
                ProductCategory newProductCategory = new ProductCategory { Id = Guid.NewGuid(), Name = fields[0].Value, MidCategoryId = SelectedMidCategory.Id };
                var testProductCategory = allProductCategories.Where(pc => pc.Name == newProductCategory.Name && pc.MidCategoryId == SelectedMidCategory.Id).FirstOrDefault();
                if (testProductCategory != null)
                {
                    DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                    SelectedTopCategory = allTopCategories.Where(tc => tc.Id == allMidCategories.Where(mc => mc.Id == newProductCategory.MidCategoryId).FirstOrDefault().Id).FirstOrDefault();
                    SelectedMidCategory = MidCategories.Where(mc => mc.Id == testProductCategory.MidCategoryId).FirstOrDefault();
                    SelectedProductCategory = ProductCategories.Where(pc => pc.Name == newProductCategory.Name).FirstOrDefault();
                    return;
                }
                using (MarketDbContext db = new MarketDbContext())
                {
                    db.ProductCategories.Add(newProductCategory);
                    await db.SaveChangesAsync();
                }

                allProductCategories.Add(newProductCategory);
                ProductCategories = new ObservableCollection<ProductCategory>(allProductCategories.Where(pc => pc.MidCategoryId == SelectedMidCategory.Id).OrderBy(pc => pc.Name));
                SelectedProductCategory = newProductCategory;
            }
        }

        private async void EditProductCategory()
        {
            List<ElementField> fields = new List<ElementField> { new ElementField("Название", SelectedProductCategory.Name) };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedProductCategory.Name = fields[0].Value;
                    db.ProductCategories.Update(SelectedProductCategory);
                    await db.SaveChangesAsync();
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
                    }
                    allProductCategories.Remove(SelectedProductCategory);
                    ProductCategories.Remove(SelectedProductCategory);
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
                    }
                    allProductCategories.RemoveAll(pc => unusedProductCategories.Select(upc => upc.Id).Contains(pc.Id));
                    ProductCategories.RemoveAll(pc => unusedProductCategories.Select(upc => upc.Id).Contains(pc.Id));
                }
            }
            else
            {
                DialogService.ShowMessageDialog("Все элементы в списке используются", "Удаление невозможно");
            }
        }

        private void SearchProductCategories()
        {
            if (!string.IsNullOrEmpty(SearchProductCategoriesText))
            {
                var foundProductCategory = allProductCategories.Where(pc => EF.Functions.Like(pc.Name, $"%{SearchProductCategoriesText}%")).FirstOrDefault();
                if (foundProductCategory != null)
                {
                    SelectedTopCategory = allTopCategories.Where(tc => tc.Id == allMidCategories.Where(mc => mc.Id == foundProductCategory.MidCategoryId).FirstOrDefault().Id).FirstOrDefault();
                    SelectedMidCategory = MidCategories.Where(tc => tc.Id == foundProductCategory.MidCategoryId).FirstOrDefault();
                    SelectedProductCategory = foundProductCategory;
                }
            }
        }

        private async void AddMidCategory()
        {
            if (SelectedTopCategory == null) return;

            List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", "")
            };

            fields = DialogService.ShowAddEditElementDlg(fields, false);
            if (fields != null)
            {
                MidCategory newMidCategory = new MidCategory { Id = Guid.NewGuid(), Name = fields[0].Value, TopCategoryId = SelectedTopCategory.Id };
                var testMidCategory = allMidCategories.Where(vt => vt.Name == newMidCategory.Name && vt.TopCategoryId == SelectedTopCategory.Id).FirstOrDefault();
                if (testMidCategory != null)
                {
                    DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                    SelectedTopCategory = TopCategories.Where(tc => tc.Id == testMidCategory.TopCategoryId).FirstOrDefault();
                    SelectedMidCategory = MidCategories.Where(vt => vt.Name == newMidCategory.Name).FirstOrDefault();
                    return;
                }

                using (MarketDbContext db = new MarketDbContext())
                {
                    db.MidCategories.Add(newMidCategory);
                    await db.SaveChangesAsync();
                }
                allMidCategories.Add(newMidCategory);
                MidCategories = new ObservableCollection<MidCategory>(allMidCategories.Where(amc => amc.TopCategoryId == newMidCategory.TopCategoryId).OrderBy(amc => amc.Name));
                SelectedMidCategory = newMidCategory;
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
                    }
                    allMidCategories.Remove(SelectedMidCategory);
                    MidCategories.Remove(SelectedMidCategory);
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
                    }
                    allMidCategories.RemoveAll(mc => unusedMidCategories.Select(umc => umc.Id).Contains(mc.Id));
                    MidCategories.RemoveAll(mc => unusedMidCategories.Select(umc => umc.Id).Contains(mc.Id));
                }
            }
            else
            {
                DialogService.ShowMessageDialog("Все элементы в списке используются", "Удаление невозможно");
            }
        }

        private void SearchMidCategories()
        {
            if (!string.IsNullOrEmpty(SearchMidCategoriesText))
            {
                var foundMidCategory = allMidCategories.Where(mc => EF.Functions.Like(mc.Name, $"%{SearchMidCategoriesText}%")).FirstOrDefault();
                if (foundMidCategory != null)
                {
                    SelectedTopCategory = TopCategories.Where(tc => tc.Id == foundMidCategory.TopCategoryId).FirstOrDefault();
                    SelectedMidCategory = foundMidCategory;
                }
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
                TopCategory newTopCategory = new TopCategory { Id = Guid.NewGuid(), Name = fields[0].Value };
                if (allTopCategories.Where(vt => vt.Name == newTopCategory.Name).FirstOrDefault() != null)
                {
                    DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                    SelectedTopCategory = TopCategories.Where(tc => tc.Name == newTopCategory.Name).FirstOrDefault();
                    return;
                }
                using (MarketDbContext db = new MarketDbContext())
                {
                    db.TopCategories.Add(newTopCategory);
                    await db.SaveChangesAsync();
                }
                allTopCategories.Add(newTopCategory);
                TopCategories = new ObservableCollection<TopCategory>(allTopCategories.OrderBy(atp => atp.Name));
                SelectedTopCategory = newTopCategory;
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
                    }
                    FTPManager.DeleteFile(FTPManager.GetTopCategoryPictureUri(SelectedTopCategory.Id));
                    allTopCategories.Remove(SelectedTopCategory);
                    TopCategories.Remove(SelectedTopCategory);
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
                    }
                    foreach (var topCat in unusedTopCategories)
                    {
                        FTPManager.DeleteFile(FTPManager.GetTopCategoryPictureUri(topCat.Id));
                    }
                    allTopCategories.RemoveAll(tc => unusedTopCategories.Select(ut => ut.Id).Contains(tc.Id));
                    TopCategories.RemoveAll(tc => unusedTopCategories.Select(ut => ut.Id).Contains(tc.Id));
                }
            }
            else
            {
                DialogService.ShowMessageDialog("Все элементы в списке используются", "Удаление невозможно");
            }
        }

        private void SearchTopCategories()
        {
            if (!string.IsNullOrEmpty(SearchTopCategoriesText))
            {
                var foundTopCategory = TopCategories.Where(tc => EF.Functions.Like(tc.Name, $"%{SearchTopCategoriesText}%")).FirstOrDefault();
                if (foundTopCategory != null) SelectedTopCategory = foundTopCategory;
            }
        }

        private void UploadTopCategoryPicture()
        {
            if (DialogService.ShowOpenPictureDialog())
            {
                var res = ImageProcessor.ResizeAndConvertProductImageToJpeg(new Uri(DialogService.FilePath), FTPManager.GetTopCategoryPictureUri(SelectedTopCategory.Id), CoreSettings.TopCategoryPictureWidth, CoreSettings.TopCategoryPictureHeight, false);
                if (res == false)
                {
                    DialogService.ShowMessageDialog("Неподдерживаемый формат файла", "Ошибка");
                }
                else
                {
                    SelectedTopCategory.OnPropertyChanged("PictureUri");
                }
            }
        }

        private List<MatchProductCategory> allProductCategoriesToMatchCategories;
        private List<ProductCategory> allProductCategories;
        private List<MidCategory> allMidCategories;
        private List<TopCategory> allTopCategories;

        public async void QueryDb()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                allProductCategoriesToMatchCategories = await db.MatchProductCategories.Include(mpt => mpt.Supplier).AsNoTracking().ToListAsync();
                allProductCategories = await db.ProductCategories.AsNoTracking().ToListAsync();
                allMidCategories = await db.MidCategories.AsNoTracking().ToListAsync();
                allTopCategories = await db.TopCategories.AsNoTracking().ToListAsync();

                TopCategories = new ObservableCollection<TopCategory>(allTopCategories.OrderBy(tc => tc.Name));
                if (TopCategories.Count > 0) SelectedTopCategory = TopCategories[0];

                ProductCategoriesToMatch = new ObservableCollection<MatchProductCategory>(allProductCategoriesToMatchCategories.Where(mpc => mpc.ProductCategoryId == null).OrderBy(mpc => SortType == 0 ? mpc.SupplierProductCategoryName : mpc.Supplier.ShortName));

                UncheckedCount = allProductCategoriesToMatchCategories.Count();
            }
        }

        public CommandType AddNewProductCategoryBasedOnMatchCommand { get; }
        public CommandType MatchProductCategoriesCommand { get; }

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

        public CommandType AddMidCategoryCommand { get; }
        public CommandType EditMidCategoryCommand { get; }
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

        public CommandType UploadTopCategoryPictureCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }
        public CommandType ShowNextPageCommand { get; }

        public CommandType ShowPositionOffersCommand { get; }
        public CommandType ShowMatchedPositionDependenciesCommand { get; }
        public CommandType CopyToClipboardCommand { get; }

        public CategoriesPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchProductCategoriesText = "";
            SearchMatchProductCategoriesText = "";

            ShowPositionOffersCommand = new CommandType();
            ShowPositionOffersCommand.Create(_ => DialogService.ShowPositionOffers(SelectedMatchProductCategory), _ => SelectedMatchProductCategory != null);
            ShowMatchedPositionDependenciesCommand = new CommandType();
            ShowMatchedPositionDependenciesCommand.Create(_ => DialogService.ShowPositionOffers(SelectedProductCategory), _ => SelectedProductCategory != null);

            RemoveMatchProductCategoryCommand = new CommandType();
            RemoveMatchProductCategoryCommand.Create(_ => RemoveMatchProductCategory(), _ => SelectedMatchProductCategory != null);
            RemoveUnusedMatchProductCategoriesCommand = new CommandType();
            RemoveUnusedMatchProductCategoriesCommand.Create(_ => RemoveUnusedMatchProductCategories());
            SearchMatchProductCategoriesCommand = new CommandType();
            SearchMatchProductCategoriesCommand.Create(_ => SearchMatchProductCategories());
            CancelSearchMatchProductCategoriesCommand = new CommandType();
            CancelSearchMatchProductCategoriesCommand.Create(_ => SearchMatchProductCategoriesText = "", _ => SearchMatchProductCategoriesText != "");

            AddProductCategoryCommand = new CommandType();
            AddProductCategoryCommand.Create(_ => AddProductCategory(), _ => SelectedMidCategory != null && SelectedTopCategory != null);
            EditProductCategoryCommand = new CommandType();
            EditProductCategoryCommand.Create(_ => EditProductCategory(), _ => SelectedProductCategory != null);
            RemoveProductCategoryCommand = new CommandType();
            RemoveProductCategoryCommand.Create(_ => RemoveProductCategory(), _ => SelectedProductCategory != null);
            RemoveUnusedProductCategoriesCommand = new CommandType();
            RemoveUnusedProductCategoriesCommand.Create(_ => RemoveUnusedProductCategories());
            SearchProductCategoriesCommand = new CommandType();
            SearchProductCategoriesCommand.Create(_ => SearchProductCategories());
            CancelSearchProductCategoriesCommand = new CommandType();
            CancelSearchProductCategoriesCommand.Create(_ => SearchProductCategoriesText = "", _ => SearchProductCategoriesText != "");

            AddNewProductCategoryBasedOnMatchCommand = new CommandType();
            AddNewProductCategoryBasedOnMatchCommand.Create(_ => AddNewProductCategoryBasedOnMatch(), _ => SelectedMatchProductCategory != null && SelectedMidCategory != null && SelectedTopCategory != null);
            MatchProductCategoriesCommand = new CommandType();
            MatchProductCategoriesCommand.Create(_ => MatchProductCategories(), _ => SelectedProductCategory != null && SelectedMatchProductCategory != null);

            AddMidCategoryCommand = new CommandType();
            AddMidCategoryCommand.Create(_ => AddMidCategory(), _ => SelectedTopCategory != null);
            EditMidCategoryCommand = new CommandType();
            EditMidCategoryCommand.Create(_ => EditMidCategory(), _ => SelectedMidCategory != null);
            RemoveMidCategoryCommand = new CommandType();
            RemoveMidCategoryCommand.Create(_ => RemoveMidCategory(), _ => SelectedMidCategory != null);
            RemoveUnusedMidCategoriesCommand = new CommandType();
            RemoveUnusedMidCategoriesCommand.Create(_ => RemoveUnusedMidCategories());
            SearchMidCategoriesCommand = new CommandType();
            SearchMidCategoriesCommand.Create(_ => SearchMidCategories());
            CancelSearchMidCategoriesCommand = new CommandType();
            CancelSearchMidCategoriesCommand.Create(_ => SearchMidCategoriesText = "", _ => SearchMidCategoriesText != "");

            AddTopCategoryCommand = new CommandType();
            AddTopCategoryCommand.Create(_ => AddTopCategory());
            EditTopCategoryCommand = new CommandType();
            EditTopCategoryCommand.Create(_ => EditTopCategory(), _ => SelectedTopCategory != null);
            RemoveTopCategoryCommand = new CommandType();
            RemoveTopCategoryCommand.Create(_ => RemoveTopCategory(), _ => SelectedTopCategory != null);
            RemoveUnusedTopCategoriesCommand = new CommandType();
            RemoveUnusedTopCategoriesCommand.Create(_ => RemoveUnusedTopCategories());
            SearchTopCategoriesCommand = new CommandType();
            SearchTopCategoriesCommand.Create(_ => SearchTopCategories());
            CancelSearchTopCategoriesCommand = new CommandType();
            CancelSearchTopCategoriesCommand.Create(_ => SearchTopCategoriesText = "", _ => SearchTopCategoriesText != "");
            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowProductExtraPropertyTypesPage());
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowOffersPage());
            UploadTopCategoryPictureCommand = new CommandType();
            UploadTopCategoryPictureCommand.Create(_ => UploadTopCategoryPicture(), _ => SelectedTopCategory != null);

            CopyToClipboardCommand = new CommandType();
            CopyToClipboardCommand.Create(_ => ClipboardService.SetText(SelectedMatchProductCategory.SupplierProductCategoryName));

            QueryDb(); //Query with true,false executes when ShowUncheckedOnly property is set
        }

    }
}
