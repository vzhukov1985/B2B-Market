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
    public class OffersPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ObservableCollection<MatchOffer> _offersToMatch;
        public ObservableCollection<MatchOffer> OffersToMatch
        {
            get { return _offersToMatch; }
            set
            {
                _offersToMatch = value;
                OnPropertyChanged("OffersToMatch");
            }
        }

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged("Products");
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

        private MatchOffer _selectedMatchOffer;
        public MatchOffer SelectedMatchOffer
        {
            get { return _selectedMatchOffer; }
            set
            {
                _selectedMatchOffer = value;

                if (Products != null && _selectedMatchOffer != null && _selectedMatchOffer.OfferId != null)
                {
                    SelectedProduct = Products.Where(p => p.Id == allOffers.Where(o => o.Id == SelectedMatchOffer.OfferId).Select(o => o.ProductId).FirstOrDefault()).FirstOrDefault();
                    SelectedOfferQuantityUnit = allOffers.Where(o => o.Id == _selectedMatchOffer.OfferId).FirstOrDefault().QuantityUnit;
                }
                else
                {
                    SelectedProduct = null;
                    SelectedOfferQuantityUnit = null;
                }

                OnPropertyChanged("SelectedMatchOffer");
            }
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                OnPropertyChanged("SelectedProduct");
            }
        }

        private string _searchMatchOffersText;
        public string SearchMatchOffersText
        {
            get { return _searchMatchOffersText; }
            set
            {
                _searchMatchOffersText = value;
                OnPropertyChanged("SearchMatchOffersText");
            }
        }

        private string _searchProductsText;
        public string SearchProductsText
        {
            get { return _searchProductsText; }
            set
            {
                _searchProductsText = value;
                OnPropertyChanged("SearchProductsText");
            }
        }

        private QuantityUnit _selectedOfferQuantityUnit;
        public QuantityUnit SelectedOfferQuantityUnit
        {
            get { return _selectedOfferQuantityUnit; }
            set
            {
                _selectedOfferQuantityUnit = value;
                OnPropertyChanged("SelectedOfferQuantityUnit");
            }
        }


        private List<ProductCategory> availableCategories;
        private List<VolumeType> availableVolumeTypes;
        private List<VolumeUnit> availableVolumeUnits;
        private List<ProductExtraPropertyType> availableProductExtraPropertyTypes;
        private List<QuantityUnit> availableQuantityUnits;
        private List<Offer> allOffers;

        private void MatchUnmatchedPicture()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                UnmatchedPic unmatchedPicRecord = db.UnmatchedPics.Where(up => up.SupplierProductCode == SelectedMatchOffer.SupplierProductCode && up.SupplierId == SelectedMatchOffer.SupplierId).FirstOrDefault();
                if (unmatchedPicRecord != null)
                {
                    bool? resultTryToMoveToMatched = FTPManager.MoveUnmatchedProductPicToMatched(unmatchedPicRecord.Id, SelectedProduct.Id);
                    if (resultTryToMoveToMatched == false) //Matched picture already exists
                    {
                        //Check if umatched pic and existing matched pic are the same
                        if (FTPManager.AreUmatchedAndMatchedPicsTheSame(unmatchedPicRecord.Id, SelectedProduct.Id) == true)
                        {
                            //DeleteUnmatchedPic
                            FTPManager.RemoveUnmatchedPic(unmatchedPicRecord.Id);
                        }
                        else
                        {
                            Guid newConflictedGuid = Guid.NewGuid();
                            if (FTPManager.MoveUnmatchedProductPicToConflicted(unmatchedPicRecord.Id, newConflictedGuid) != null)
                            {
                                db.ConflictedPics.Add(new ConflictedPic { Id = newConflictedGuid, SupplierId = SelectedMatchOffer.SupplierId, ProductId = SelectedProduct.Id });
                                db.SaveChanges();
                            }
                        }
                    }
                    if (resultTryToMoveToMatched != null) //If moving to matched or conflicted is ok
                    {
                        db.UnmatchedPics.Remove(unmatchedPicRecord);
                        db.SaveChanges();
                    }
                }
            }
        }

        private void MatchUnmatchedDesc()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                UnmatchedDescription unmatchedDescRecord = db.UnmatchedDescriptions.Where(up => up.SupplierProductCode == SelectedMatchOffer.SupplierProductCode && up.SupplierId == SelectedMatchOffer.SupplierId).FirstOrDefault();
                SelectedProduct.Description = db.ProductDescriptions.Find(SelectedProduct.Id);
                if (unmatchedDescRecord != null)
                {
                    if (SelectedProduct.Description != null && SelectedProduct.Description.Text != "")
                    {
                        if (SelectedProduct.Description.Text != unmatchedDescRecord.Description)
                        {
                            Guid newConflictedGuid = Guid.NewGuid();
                            db.ConflictedDescriptions.Add(new ConflictedDescription { Id = newConflictedGuid, SupplierId = SelectedMatchOffer.SupplierId, ProductId = SelectedProduct.Id, Description = unmatchedDescRecord.Description });
                        }
                    }
                    else
                    {
                        if (SelectedProduct.Description == null)
                        {
                            db.ProductDescriptions.Add(new ProductDescription { ProductId = SelectedProduct.Id, Text = unmatchedDescRecord.Description });
                        }
                        else
                        {
                            db.ProductDescriptions.Update(SelectedProduct.Description);
                        }
                    }
                    db.UnmatchedDescriptions.Remove(unmatchedDescRecord);
                    db.SaveChanges();

                }
            }
        }

        private void RemoveMatchOffer()
        {
            /*          List<Guid> unusedProductsIds = MarketDbContext.GetUnusedMatchOffersIds();
                       if (unusedProductsIds.Contains(SelectedMatchOffer.Id))
                       {
                           Tuple<string, string> element = new Tuple<string, string>(SelectedMatchOffer.Supplier.ShortName, "\"" + SelectedMatchOffer.SupplierProductName + "\"");
                           if (DialogService.ShowWarningElementsRemoveDialog(new List<Tuple<string, string>> { element }))
                           {
                               using (MarketDbContext db = new MarketDbContext())
                               {
                                   db.MatchOffers.Remove(SelectedMatchOffer);
                                   await db.SaveChangesAsync();
                                   _ = QueryDb(true, false);
                               }
                           }
                       }
                       else
                       {
                           DialogService.ShowMessageDialog("Элемент не может быть удален, т.к. используется", "Удаление невозможно");
                       }*/
        }

        private void RemoveUnusedMatchOffers()
        {
            /*            List<MatchOffer> unusedProducts = MarketDbContext.GetUnusedMatchOffers();
                        if (unusedProducts.Count > 0)
                        {
                            List<Tuple<string, string>> elements = unusedProducts.Select(vt => new Tuple<string, string>(vt.Supplier.ShortName, "\"" + vt.SupplierProductName + "\"")).ToList();
                            if (DialogService.ShowWarningElementsRemoveDialog(elements))
                            {
                                using (MarketDbContext db = new MarketDbContext())
                                {
                                    db.MatchOffers.RemoveRange(unusedProducts);
                                    await db.SaveChangesAsync();
                                    _ = QueryDb(true, false);
                                }
                            }
                        }
                        else
                        {
                            DialogService.ShowMessageDialog("Все элементы в списке используются", "Удаление невозможно");
                        }*/
        }

        private async void AddNewProductBasedOnMatch()
        {

            Guid newProductId = Guid.NewGuid();
            Product newProduct = new Product()
            {
                Id = newProductId,
                Name = SelectedMatchOffer.ProductName,
                Code = Products == null || Products.Count == 0 ? 1 : Products.Max(p => p.Code)+1,
                Category = availableCategories.Where(pc => pc.Id == SelectedMatchOffer.MatchProductCategory.ProductCategoryId).FirstOrDefault(),
                VolumeType = availableVolumeTypes.Where(vt => vt.Id == SelectedMatchOffer.MatchVolumeType.VolumeTypeId).FirstOrDefault(),
                VolumeUnit = availableVolumeUnits.Where(vu => vu.Id == SelectedMatchOffer.MatchVolumeUnit.VolumeUnitId).FirstOrDefault(),
                Volume = SelectedMatchOffer.ProductVolume,
                ExtraProperties = new ObservableCollection<ProductExtraProperty>(SelectedMatchOffer.MatchProductExtraProperties.Select(mpep => new ProductExtraProperty
                {
                    Id = Guid.NewGuid(),
                    ProductId = newProductId,
                    PropertyType = availableProductExtraPropertyTypes.Where(pep => pep.Id == mpep.MatchProductExtraPropertyType.ProductExtraPropertyTypeId).FirstOrDefault(),
                    Value = mpep.Value
                }))
            };

            Offer newOffer = new Offer
            {
                Id = Guid.NewGuid(),
                Product = newProduct,
                QuantityUnit = availableQuantityUnits.Where(qu => qu.Id == SelectedMatchOffer.MatchQuantityUnit.QuantityUnitId).FirstOrDefault(),
                Supplier = SelectedMatchOffer.Supplier,
                SupplierProductCode = SelectedMatchOffer.SupplierProductCode,
                Remains = SelectedMatchOffer.Remains,
                RetailPrice = SelectedMatchOffer.RetailPrice,
                DiscountPrice = SelectedMatchOffer.DiscountPrice,
                IsActive = true
            };

            bool dlgResult = DialogService.ShowMatchOfferDlg(
                SelectedMatchOffer,
                newOffer,
                availableCategories,
                availableVolumeTypes,
                availableVolumeUnits,
                availableProductExtraPropertyTypes,
                availableQuantityUnits
                );

            if (dlgResult == true)
            {
                Offer oldOffer;
                using (MarketDbContext db = new MarketDbContext())
                {
                    db.Products.Add(Product.CloneForDB(newOffer.Product));
                    db.ProductExtraProperties.AddRange(newOffer.Product.ExtraProperties.Select(ep => ProductExtraProperty.CloneForDB(ep)));
                    db.Offers.Add(Offer.CloneForDB(newOffer));
                    await db.SaveChangesAsync();
                }
                using (MarketDbContext db = new MarketDbContext())
                {
                    oldOffer = await db.Offers.FindAsync(SelectedMatchOffer.OfferId);
                    SelectedMatchOffer.OfferId = newOffer.Id;
                    db.MatchOffers.Update(MatchOffer.CloneForDB(SelectedMatchOffer));
                    await db.SaveChangesAsync();
                }


                if (oldOffer != null)
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.Offers.Remove(Offer.CloneForDB(oldOffer));
                        await db.SaveChangesAsync();
                    }
                }

                using (MarketDbContext db = new MarketDbContext())
                {
                    newOffer.Product.Code = db.Products.Find(newOffer.ProductId).Code;
                }
                allOffers.Add(newOffer);
                Products.Add(newOffer.Product);
                SelectedProduct = newOffer.Product;
                MatchUnmatchedPicture();
                MatchUnmatchedDesc();

                UncheckedCount--;
                if (ShowUncheckedOnly)
                    OffersToMatch.Remove(SelectedMatchOffer);
            }
            SelectedProduct = newOffer.Product;
        }

        private async void MatchOffers()
        {
            SelectedProduct.Category = availableCategories.Where(ac => ac.Id == SelectedProduct.CategoryId).FirstOrDefault();
            SelectedProduct.VolumeType = availableVolumeTypes.Where(avt => avt.Id == SelectedProduct.VolumeTypeId).FirstOrDefault();
            SelectedProduct.VolumeUnit = availableVolumeUnits.Where(avu => avu.Id == SelectedProduct.VolumeUnitId).FirstOrDefault();
            foreach (ProductExtraProperty ep in SelectedProduct.ExtraProperties)
                ep.PropertyType = availableProductExtraPropertyTypes.Where(ept => ept.Id == ep.PropertyTypeId).FirstOrDefault();


            Offer offer;
            bool updateOffer;
            if (SelectedMatchOffer.OfferId != null)
            {
                offer = allOffers.Where(o => o.Id == SelectedMatchOffer.OfferId).FirstOrDefault();
                updateOffer = true;
            }
            else
            {
                offer = new Offer { Id = Guid.NewGuid() };
                updateOffer = false;
            }
            QuantityUnit oldOfferQuantityUnit = offer.QuantityUnit;
            offer.QuantityUnit = availableQuantityUnits.Where(qu => qu.Id == SelectedMatchOffer.MatchQuantityUnit.QuantityUnitId).FirstOrDefault();
            offer.Product = SelectedProduct;
            offer.Supplier = SelectedMatchOffer.Supplier;
            offer.SupplierProductCode = SelectedMatchOffer.SupplierProductCode;
            offer.Remains = SelectedMatchOffer.Remains;
            offer.RetailPrice = SelectedMatchOffer.RetailPrice;
            offer.DiscountPrice = SelectedMatchOffer.DiscountPrice;
            offer.IsActive = true;


            List<ProductExtraProperty> oldProperties = SelectedProduct.ExtraProperties.ToList();

            bool dlgResult = DialogService.ShowMatchOfferDlg(
                SelectedMatchOffer,
                offer,
                availableCategories,
                availableVolumeTypes,
                availableVolumeUnits,
                availableProductExtraPropertyTypes,
                availableQuantityUnits
                );


            if (dlgResult == true)
            {
                //Matching Product Changes
                using (MarketDbContext db = new MarketDbContext())
                {
                    db.Products.Update(Product.CloneForDB(SelectedProduct));
                    await db.SaveChangesAsync();
                }

                List<ProductExtraProperty> newProperties = SelectedProduct.ExtraProperties.ToList();

                using (MarketDbContext db = new MarketDbContext())
                {
                    List<ProductExtraProperty> propertiesToUpdate = oldProperties.Intersect(newProperties).Select(p => ProductExtraProperty.CloneForDB(p)).ToList();
                    List<ProductExtraProperty> propertiesToAdd = newProperties.Except(oldProperties).Select(p => ProductExtraProperty.CloneForDB(p)).ToList();
                    List<ProductExtraProperty> propertiesToDelete = oldProperties.Except(newProperties).Select(p => ProductExtraProperty.CloneForDB(p)).ToList();
                    db.ProductExtraProperties.UpdateRange(propertiesToUpdate);
                    db.ProductExtraProperties.AddRange(propertiesToAdd);
                    db.ProductExtraProperties.RemoveRange(propertiesToDelete);
                    await db.SaveChangesAsync();
                }

                //Matching Offers
                using (MarketDbContext db = new MarketDbContext())
                {
                    if (updateOffer)
                    {
                        db.Offers.Update(Offer.CloneForDB(offer));
                    }
                    else
                    {
                        db.Offers.Add(Offer.CloneForDB(offer));
                    }
                    await db.SaveChangesAsync();
                }

                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedMatchOffer.OfferId = offer.Id;
                    db.MatchOffers.Update(MatchOffer.CloneForDB(SelectedMatchOffer));
                    await db.SaveChangesAsync();
                }

                MatchUnmatchedPicture();
                MatchUnmatchedDesc();

                if (updateOffer == false)
                {
                    allOffers.Add(offer);
                    UncheckedCount--;
                    if (ShowUncheckedOnly)
                        OffersToMatch.Remove(SelectedMatchOffer);
                }

            }
            else
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    int index = Products.IndexOf(SelectedProduct);
                    Products[index] = await db.Products
                        .AsNoTracking()
                        .Include(p => p.Category)
                        .Include(p => p.ExtraProperties)
                        .ThenInclude(ep => ep.PropertyType)
                        .Include(p => p.VolumeType)
                        .Include(p => p.VolumeUnit)
                        .Where(p => p.Id == SelectedProduct.Id).FirstOrDefaultAsync();
                    SelectedProduct = Products[index];
                    offer.QuantityUnit = oldOfferQuantityUnit;
                }
            }
            SelectedOfferQuantityUnit = offer.QuantityUnit;
        }

        private void AddProduct()
        {
            /*List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", "")
            };

            fields = DialogService.ShowAddEditElementDlg(fields, false);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    Product newProduct = new Product { Id = Guid.NewGuid(), Name = fields[0].Value };
                    if (db.Products.Where(vt => vt.Name == newProduct.Name).FirstOrDefault() != null)
                    {
                        DialogService.ShowMessageDialog("Позиция с такими же параметрами уже существует. Добавление невозможно", "Ошибка");
                        if (Products.Where(vt => vt.Name == newProduct.Name).FirstOrDefault() == null)
                        {
                            SearchProductsText = "";
                        }
                        await QueryDb(false, true);
                        SelectedProduct = Products.Where(vt => vt.Name == newProduct.Name).FirstOrDefault();
                        return;
                    }

                    db.Products.Add(newProduct);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }*/
        }

        private void EditProduct()
        {
            /*List<ElementField> fields = new List<ElementField>{
                new ElementField("Название", SelectedProduct.Name)
            };

            fields = DialogService.ShowAddEditElementDlg(fields, true);
            if (fields != null)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    SelectedProduct.Name = fields[0].Value;
                    db.Products.Update(SelectedProduct);
                    await db.SaveChangesAsync();
                    _ = QueryDb(false, true);
                }
            }*/
        }

        private async void RemoveProduct()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                
                if (db.Offers.Where(o => o.ProductId == SelectedProduct.Id).Count() == 0)
                {
                    if (DialogService.ShowOkCancelDialog("ВНИМАНИЕ!!! Вы уверены, что хотите удалить продукт \"" + SelectedProduct.Name + "\"?", "ВНИМАНИЕ!!!"))
                    {
                        db.ProductExtraProperties.RemoveRange(SelectedProduct.ExtraProperties.Select(ep => ProductExtraProperty.CloneForDB(ep)));
                        var prodDesc = db.ProductDescriptions.Find(SelectedProduct.Id);
                        if (prodDesc != null)
                            db.ProductDescriptions.Remove(prodDesc);
                        db.Products.Remove(Product.CloneForDB(SelectedProduct));
                        await db.SaveChangesAsync();
                        Products.Remove(SelectedProduct);
                    }
                }
                else
                {
                    DialogService.ShowMessageDialog("Невозможно удалить продукт, т.к. он есть в предложениях от поставщиков.", "Ошибка");
                }
            }

        }

        private void RemoveUnusedProducts()
        {
/*            List<Product> unusedProducts = MarketDbContext.GetUnusedProducts();
            if (unusedProducts.Count > 0)
            {
                List<Tuple<string, string>> elements = unusedProducts.Select(vt => new Tuple<string, string>("", "\"" + vt.Name + "\"")).ToList();
                if (DialogService.ShowWarningElementsRemoveDialog(elements))
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.ProductExtraProperties.RemoveRange(Products.Select(p => p.ExtraProperties))
                        db.Products.RemoveRange(unusedProducts);
                        await db.SaveChangesAsync();
                        _ = QueryDb(false, true);
                    }
                }
            }
            else
            {
                DialogService.ShowMessageDialog("Все элементы в списке используются", "Удаление невозможно");
            }*/
        }



        public async Task QueryDb(bool UpdateOffersToMatch = true, bool UpdateProducts = true)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                if (UpdateOffersToMatch)
                {
                    OffersToMatch = new ObservableCollection<MatchOffer>(await db.MatchOffers
                        .Include(otm => otm.Supplier)
                        .Include(otm => otm.MatchProductCategory)
                        .Include(otm => otm.MatchVolumeType)
                        .Include(otm => otm.MatchVolumeUnit)
                        .Include(otm => otm.MatchQuantityUnit)
                        .Include(otm => otm.MatchProductExtraProperties)
                        .ThenInclude(mpep => mpep.MatchProductExtraPropertyType)
                        .Where(otm => ShowUncheckedOnly ? otm.OfferId == null : true)
                        .Where(otm => SearchMatchOffersText == null ? true :
                            EF.Functions.Like(otm.MatchProductCategory.SupplierProductCategoryName, $"%{SearchMatchOffersText}%") ||
                            EF.Functions.Like(otm.ProductName, $"%{SearchMatchOffersText}%") ||
                            EF.Functions.Like(otm.SupplierProductCode, $"%{SearchMatchOffersText}%") ||
                            EF.Functions.Like(otm.Supplier.ShortName, $"%{SearchMatchOffersText}%"))
                        .AsNoTracking()
                        .ToListAsync()
                         );
                    UncheckedCount = await db.MatchOffers.Where(mo => mo.OfferId == null).CountAsync();
                }

                if (UpdateProducts)
                {

                    Products = new ObservableCollection<Product>(await db.Products
                        .Include(p => p.Category)
                        .Include(p => p.ExtraProperties)
                        .ThenInclude(ep => ep.PropertyType)
                        .Include(p => p.VolumeType)
                        .Include(p => p.VolumeUnit)
                        .Where(p => SearchProductsText == null ? true :
                            EF.Functions.Like(p.Name, $"%{SearchProductsText}%") ||
                            EF.Functions.Like(p.Category.Name, $"%{SearchProductsText}%"))
                        .AsNoTracking()
                        .ToListAsync()
                        );
                }
            }
        }

        private async Task QueryAvailableItems()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                availableCategories = await db.ProductCategories.AsNoTracking().ToListAsync();
                availableVolumeTypes = await db.VolumeTypes.AsNoTracking().ToListAsync();
                availableVolumeUnits = await db.VolumeUnits.AsNoTracking().ToListAsync();
                availableProductExtraPropertyTypes = await db.ProductExtraPropertyTypes.AsNoTracking().ToListAsync();
                availableQuantityUnits = await db.QuantityUnits.AsNoTracking().ToListAsync();
                allOffers = await db.Offers.AsNoTracking().Include(o => o.QuantityUnit).ToListAsync();
            }
        }

        public CommandType RemoveMatchOfferCommand { get; }
        public CommandType RemoveUnusedMatchOffersCommand { get; }
        public CommandType SearchMatchOffersCommand { get; }
        public CommandType CancelSearchMatchOffersCommand { get; }

        public CommandType AddProductCommand { get; }
        public CommandType EditProductCommand { get; }
        public CommandType RemoveProductCommand { get; }
        public CommandType RemoveUnusedProductsCommand { get; }
        public CommandType SearchProductsCommand { get; }
        public CommandType CancelSearchProductsCommand { get; }

        public CommandType AddNewProductBasedOnMatchCommand { get; }
        public CommandType MatchOffersCommand { get; }

        public CommandType ShowPreviousPageCommand { get; }
        public CommandType ShowNextPageCommand { get; }

        public OffersPageVM(IPageService pageService, IDialogService dialogService)
        {
            PageService = pageService;
            DialogService = dialogService;

            ShowUncheckedOnly = true;
            SearchProductsText = "";
            SearchMatchOffersText = "";

            RemoveMatchOfferCommand = new CommandType();
            RemoveMatchOfferCommand.Create(_ => RemoveMatchOffer(), _ => SelectedMatchOffer != null);
            RemoveUnusedMatchOffersCommand = new CommandType();
            RemoveUnusedMatchOffersCommand.Create(_ => RemoveUnusedMatchOffers());
            SearchMatchOffersCommand = new CommandType();
            SearchMatchOffersCommand.Create(_ => _ = QueryDb(true, false));
            CancelSearchMatchOffersCommand = new CommandType();
            CancelSearchMatchOffersCommand.Create(_ => { SearchMatchOffersText = ""; _ = QueryDb(true, false); }, _ => SearchMatchOffersText != "");

            AddProductCommand = new CommandType();
            AddProductCommand.Create(_ => AddProduct());
            EditProductCommand = new CommandType();
            EditProductCommand.Create(_ => EditProduct(), _ => SelectedProduct != null);
            RemoveProductCommand = new CommandType();
            RemoveProductCommand.Create(_ => RemoveProduct(), _ => SelectedProduct != null);
            RemoveUnusedProductsCommand = new CommandType();
            RemoveUnusedProductsCommand.Create(_ => RemoveUnusedProducts());
            SearchProductsCommand = new CommandType();
            SearchProductsCommand.Create(_ => _ = QueryDb(false, true));
            CancelSearchProductsCommand = new CommandType();
            CancelSearchProductsCommand.Create(_ => { SearchProductsText = ""; _ = QueryDb(false, true); }, _ => SearchProductsText != "");

            AddNewProductBasedOnMatchCommand = new CommandType();
            AddNewProductBasedOnMatchCommand.Create(_ => AddNewProductBasedOnMatch(), _ => SelectedMatchOffer != null);
            MatchOffersCommand = new CommandType();
            MatchOffersCommand.Create(_ => MatchOffers(), _ => SelectedProduct != null && SelectedMatchOffer != null);

            ShowPreviousPageCommand = new CommandType();
            ShowPreviousPageCommand.Create(_ => PageService.ShowTopCategoriesPage());
            ShowNextPageCommand = new CommandType();
            ShowNextPageCommand.Create(_ => PageService.ShowPicturesPage());

            _ = QueryDb(false, true); //Query with true,false executes when ShowUncheckedOnly property is set
            _ = QueryAvailableItems();
        }

    }
}
