using ClientApp.Services;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClientApp.ViewModels
{
    public enum ProductOrderAndRemainsState
    {
        Ok,
        OneSupplierNullRemains,
        AllSuppliersNullRemains,
        OneSupplierLessRemains,
        OneOfSuppliersLessRemains
    }

    public class ProductForRequestView : Product
    {
        private ObservableCollection<OfferWithOrder> _orders;
        public ObservableCollection<OfferWithOrder> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged("Orders");
            }
        }

        public ProductOrderAndRemainsState OrderAndRemainsState
        {
            get
            {
                if (Orders.All(o => o.Remains == 0 || o.Supplier.IsActive == false|| o.IsActive == false || o.IsChecked == false))
                { 
                    if (Orders.Count == 1)
                        return ProductOrderAndRemainsState.OneSupplierNullRemains;
                    else
                        return ProductOrderAndRemainsState.AllSuppliersNullRemains;
                }

                if (Orders.Any(o => o.OrderQuantity > o.Remains || o.Supplier.IsActive == false || o.IsActive == false || o.IsChecked == false))
                {
                    if (Orders.Count == 1)
                        return ProductOrderAndRemainsState.OneSupplierLessRemains;
                    else
                        return ProductOrderAndRemainsState.OneOfSuppliersLessRemains;
                }
                return ProductOrderAndRemainsState.Ok;
            }
        }

        public ProductForRequestView(Product product)
        {
            this.BestDiscountPriceOffer = product.BestDiscountPriceOffer;
            this.BestRetailPriceOffer = product.BestRetailPriceOffer;
            this.Category = product.Category;
            this.CategoryId = product.CategoryId;
            this.ExtraProperties = product.ExtraProperties;
            this.Favorites = product.Favorites;
            this.Id = product.Id;
            this.IsFavoriteForUser = product.IsFavoriteForUser;
            this.IsOfContractedSupplier = product.IsOfContractedSupplier;
            this.Name = product.Name;
            this.Offers = product.Offers;
            this.Volume = product.Volume;
            this.VolumeType = product.VolumeType;
            this.VolumeTypeId = product.VolumeTypeId;
            this.VolumeUnit = product.VolumeUnit;
            this.VolumeUnitId = product.VolumeUnitId;
        }
    }

    public class CategoryForRequestView : ProductCategory
    {
        private ObservableCollection<ProductForRequestView> _products;
        public ObservableCollection<ProductForRequestView> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged("Products");
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }


        public decimal Subtotal
        {
            get
            {
                return Products.Sum(p => p.Orders.Sum(o => o.PriceForClient * o.OrderQuantity));
            }
        }


        public CategoryForRequestView(TopCategory category)
        {
            this.Name = category.Name;
        }

        public CategoryForRequestView(Supplier supplier)
        {
            this.Name = supplier.ShortName;
        }
    }





    public class CurrentRequestSubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private readonly IPageService PageService;
        private readonly IDialogService DialogService;

        private ClientUser _user;
        public ClientUser User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        private ObservableCollection<ProductForRequestView> _products;
        public ObservableCollection<ProductForRequestView> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnPropertyChanged("Products");
            }
        }

        private ObservableCollection<CategoryForRequestView> _categories;
        public ObservableCollection<CategoryForRequestView> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                OnPropertyChanged("Categories");
                OnPropertyChanged("ItemsCount");
                OnPropertyChanged("ProductsNamesCount");
                OnPropertyChanged("SuppliersCount");
                OnPropertyChanged("TotalSum");
            }
        }

        private bool _isGroupingByCategories;
        public bool IsGroupingByCategories
        {
            get { return _isGroupingByCategories; }
            set
            {
                _isGroupingByCategories = value;
                OnPropertyChanged("IsGroupingByCategories");
            }
        }

        public int ItemsCount
        {
            get 
            {
                if (Categories != null)
                    return Categories.Where(c => c.IsSelected).SelectMany(c => c.Products.SelectMany(p => p.Orders)).Sum(o => o.OrderQuantity);
                return 0;
            }
        }

        public int ProductsNamesCount
        {
            get 
            {
                if (Categories != null)
                    return Categories.Where(c => c.IsSelected).SelectMany(c => c.Products.SelectMany(p => p.Orders)).GroupBy(o => o.ProductId).Count();
                return 0;
            }
        }

        public int SuppliersCount
        {
            get {
                if (Categories != null)
                    return Categories.Where(c => c.IsSelected).SelectMany(c => c.Products.SelectMany(p => p.Orders)).GroupBy(o => o.SupplierId).Count();
                return 0;
            }
        }

        public decimal TotalSum
        {
            get 
            {
                if (Categories != null)
                return Categories.Where(c => c.IsSelected).Sum(c => c.Products.Sum(p => p.Orders.Sum(o => o.PriceForClient * o.OrderQuantity)));
                return 0;
            }
        }

        public bool RequestIsReadyToProceed
        {
            get
            {
                if (Categories != null)
                {
                    var a = Categories.Where(c => c.IsSelected).SelectMany(c => c.Products.Select(p => p.OrderAndRemainsState));

                    return a.All(st => st == ProductOrderAndRemainsState.Ok) && User.IsAdmin && a.Count() > 0;
                }
                return false;
            }
        }

        private async void ReloadRequestData(bool requeryProductsDB)
        {
            List<Guid> ContractedSuppliersIds = User.Client.Contracts.Select(p => p.Supplier.Id).ToList();
            List<Guid> FavoriteProductsIds = User.FavoriteProducts.Select(f => f.Product.Id).ToList();

            Orders = User.Client.CurrentOrders.Select(o => new OfferWithOrder
            {
                Id = o.OfferId,
                SupplierProductCode = o.Offer.SupplierProductCode,
                IsActive = o.Offer.IsActive,
                IsChecked = o.Offer.IsChecked,
                SupplierId = o.Offer.SupplierId,
                Supplier = o.Offer.Supplier,
                ProductId = o.Offer.ProductId,
                Product = o.Offer.Product,
                QuantityUnitId = o.Offer.QuantityUnitId,
                QuantityUnit = o.Offer.QuantityUnit,
                Remains = o.Offer.Remains,
                RetailPrice = o.Offer.RetailPrice,
                DiscountPrice = o.Offer.DiscountPrice,
                IsOfContractedSupplier = ContractedSuppliersIds.Contains(o.Offer.SupplierId),
                OrderQuantity = o.Quantity,
                OrderQuantityBeforeUserChanges = o.Quantity,
                PriceForClient = ContractedSuppliersIds.Contains(o.Offer.SupplierId) ? o.Offer.DiscountPrice : o.Offer.RetailPrice
            }).ToList();

            List<Guid> ProductIds = User.Client.CurrentOrders.Select(o => o.Offer.ProductId).Distinct().ToList();

            if (requeryProductsDB)
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    ProductsFromDb = await db.Products
                         .Include(p => p.Offers)
                         .ThenInclude(o => o.QuantityUnit)
                         .Include(p => p.Offers)
                         .ThenInclude(o => o.Supplier)
                         .Include(p => p.ExtraProperties)
                         .ThenInclude(pr => pr.PropertyType)
                         .Include(p => p.VolumeUnit)
                         .Include(p => p.VolumeType)
                         .Include(p => p.Category)
                         .ThenInclude(pc => pc.MidCategory)
                         .ThenInclude(mc => mc.TopCategory)
                         .Where(p => ProductIds.Contains(p.Id))
                         .OrderBy(p => p.Category.MidCategory.TopCategory.Name)
                         .ThenBy(p => p.Category.Name)
                         .ThenBy(p => p.Name)
                         .ToListAsync();
                }
            }
            else
            {
                ProductsFromDb = ProductsFromDb
                         .Where(p => ProductIds.Contains(p.Id))
                         .OrderBy(p => p.Category.MidCategory.TopCategory.Name)
                         .ThenBy(p => p.Category.Name)
                         .ThenBy(p => p.Name).ToList();
            }

            if (IsGroupingByCategories)
            {
                Categories = new ObservableCollection<CategoryForRequestView>(ProductsFromDb.GroupBy(p => p.Category.MidCategory.TopCategory).Select(c => new CategoryForRequestView(c.Key)
                {
                    IsSelected = true,
                    Products = new ObservableCollection<ProductForRequestView>(ProductsFromDb.Where(p => p.Category.MidCategory.TopCategory.Id == c.Key.Id).Select(p => new ProductForRequestView(p)
                    {
                        Orders = new ObservableCollection<OfferWithOrder>(Orders.Where(o => o.ProductId == p.Id).OrderByDescending(o => ContractedSuppliersIds.Contains(o.SupplierId))),
                        IsOfContractedSupplier = p.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id)),
                        IsFavoriteForUser = FavoriteProductsIds.Contains(p.Id)
                    }))
                })) ;
            }
            else
            {
                Categories = new ObservableCollection<CategoryForRequestView>(Orders.GroupBy(c => c.Supplier).OrderByDescending(c => ContractedSuppliersIds.Contains(c.Key.Id)).ThenBy(c => c.Key.ShortName).Select(c => new CategoryForRequestView(c.Key)
                {
                    IsSelected = true,
                    Products = new ObservableCollection<ProductForRequestView>(ProductsFromDb.
                    Where(p => p.Offers.Select(o => o.SupplierId).Contains(c.Key.Id) &&
                    Orders.Where(oo => oo.SupplierId == c.Key.Id).Select(oo => oo.ProductId).Contains(p.Id))
                    .Select(p => new ProductForRequestView(p)
                    {
                        Orders = new ObservableCollection<OfferWithOrder>(Orders.Where(o => (o.ProductId == p.Id) && (o.SupplierId == c.Key.Id))),
                        IsOfContractedSupplier = p.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id)),
                        IsFavoriteForUser = FavoriteProductsIds.Contains(p.Id)
                    }))
                }));
            }
            if (RequestIsReadyToProceed == true) return;
        }

        private void ProceedRequest()
        {
            int LastRequestCode;
            using (MarketDbContext db = new MarketDbContext())
            {
                LastRequestCode = db.ArchivedRequests.Count() > 0? db.ArchivedRequests.Max(r => r.Code): 0;
            }
            List<ArchivedRequest> requestsToAdd = Categories.Where(c => c.IsSelected).SelectMany(c => c.Products.SelectMany(p => p.Orders)).GroupBy(c => c.Supplier).Select(s => new ArchivedRequest
            {
                Id = new Guid(),
                Client = User.Client,
                ClientId = User.ClientId,
                SenderName = User.Name,
                SenderSurName = User.SurName,
                ItemsQuantity = Orders.Where(o => o.SupplierId == s.Key.Id).Sum(o => o.OrderQuantity),
                ProductsQuantity = Orders.Where(o => o.SupplierId == s.Key.Id).GroupBy(o => o.ProductId).Count(),
                TotalPrice = Orders.Where(o => o.SupplierId == s.Key.Id).Sum(o => o.PriceForClient * o.OrderQuantity),
                ArchivedSupplierId = s.Key.Id,
                DateTimeSent = DateTime.Now,
                DeliveryTime = DateTime.Today.AddDays(1) + new TimeSpan(10, 0, 0),
                Comments = "",
                ArchivedSupplier = new ArchivedSupplier
                {
                    Id = s.Key.Id,
                    Address = s.Key.Address,
                    BIN = s.Key.BIN,
                    Email = s.Key.Email,
                    FullName = s.Key.FullName,
                    Phone = s.Key.Phone,
                    ShortName = s.Key.Phone
                },
                ArchivedRequestsStatuses = new ObservableCollection<ArchivedRequestsStatus>
                {
                        new ArchivedRequestsStatus { ArchivedRequestStatusTypeId = new Guid("ceff6b71-a27c-468b-b9f6-fd0ccc8d6024"), DateTime = DateTime.Now }, //SENT
                        new ArchivedRequestsStatus { ArchivedRequestStatusTypeId = new Guid("3df59a9b-4874-4aa4-83de-545fd0d0e6ec"), DateTime = DateTime.Now.AddSeconds(1) }  //PENDING
                },
                Orders = new ObservableCollection<ArchivedOrder>(Orders.Where(o => o.SupplierId == s.Key.Id).Select(o => new ArchivedOrder
                {
                    Id = new Guid(),
                    OfferId = o.Id,
                    SupplierProductCode = o.SupplierProductCode,
                    Price = o.PriceForClient,
                    QuantityUnit = o.QuantityUnit.ShortName,
                    Quantity = o.OrderQuantity,
                    ProductId = o.ProductId,
                    Product = ProductsFromDb.Where(p => p.Id == o.ProductId).FirstOrDefault(),
                }))
            }).ToList();

            foreach (ArchivedRequest request in requestsToAdd)
            {
                request.Code = LastRequestCode+1;
                LastRequestCode++;
            }

            PageService.ShowCurrentRequestConfirmSubPage(User, requestsToAdd);

        }

        private void RemoveProduct(ProductForRequestView selectedProduct)
        {
            string dialogText;
            if (IsGroupingByCategories)
                dialogText = "Вы действительно хотите удалить позицию от всех поставщиков?";
            else
                dialogText = "Вы действительно хотите удалить эту позицию от поставщика \"" + selectedProduct.Orders[0].Supplier.FullName + "\"?";

            if (DialogService.ShowOkCancelDialog(dialogText, "Внимание!") == false) return;

            List<Guid> OffersToRemoveIds = selectedProduct.Orders.Select(od => od.Id).ToList();

            using (MarketDbContext db = new MarketDbContext())
            {
                db.CurrentOrders.RemoveRange(db.CurrentOrders.Where(o => OffersToRemoveIds.Contains(o.OfferId) && o.ClientId == User.ClientId));
                db.SaveChanges();
            }
            foreach (CurrentOrder order in User.Client.CurrentOrders.ToList())
            {
                if (OffersToRemoveIds.Contains(order.OfferId) && order.ClientId == User.ClientId)
                    User.Client.CurrentOrders.Remove(order);
            }
            ReloadRequestData(false);
        }

        private void RemoveProductsCategory(CategoryForRequestView selectedCategory)
        {
            string dialogText;
            if (IsGroupingByCategories)
                dialogText = "Вы действительно хотите все выбранные товары в категории \"" + selectedCategory.Name +"\"?";
            else
                dialogText = "Вы действительно хотите все выбранные товары от поставщика \"" + selectedCategory.Name + "\"?";

            if (DialogService.ShowOkCancelDialog(dialogText, "Внимание!") == false) return;

            List<Guid> OffersToRemoveIds = selectedCategory.Products.SelectMany(p => p.Orders.Select(o => o.Id)).ToList();

            using (MarketDbContext db = new MarketDbContext())
            {
                db.CurrentOrders.RemoveRange(db.CurrentOrders.Where(o => OffersToRemoveIds.Contains(o.OfferId) && o.ClientId == User.ClientId));
                db.SaveChanges();
            }
            foreach (CurrentOrder order in User.Client.CurrentOrders.ToList())
            {
                if (OffersToRemoveIds.Contains(order.OfferId) && order.ClientId == User.ClientId)
                    User.Client.CurrentOrders.Remove(order);
            }
            ReloadRequestData(false);
        }

        private List<OfferWithOrder> Orders;
        private List<Product> ProductsFromDb;

        public CommandType ShowSearchSubPageCommand { get; }
        public CommandType AddRemoveProductToFavouritesCommand { get; }
        public CommandType ShowProductCommand { get; }
        public CommandType NavigationBackCommand { get; }
        public CommandType LoadCurrentRequestDataCommand { get; }
        public CommandType SwitchGroupingCommand { get; }
        public CommandType ProceedRequestCommand { get; }

        public CommandType RemoveProductCommand { get; }
        public CommandType RemoveProductsCategoryCommand { get; }
        public CommandType CategoriesSelectionChangedCommand { get; }


        public CurrentRequestSubPageVM(ClientUser user, IPageService pageService, IDialogService dialogService)
        {
            User = user;
            PageService = pageService;
            DialogService = dialogService;


            ShowSearchSubPageCommand = new CommandType();
            ShowSearchSubPageCommand.Create(_ => PageService.ShowSearchSubPage(User));
            AddRemoveProductToFavouritesCommand = new CommandType();
            AddRemoveProductToFavouritesCommand.Create(p => MarketDbContext.AddRemoveProductToFavourites((Product)p, User));
            ShowProductCommand = new CommandType();
            ShowProductCommand.Create(p => PageService.ShowProductSubPage(User, (Product)p));
            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());
            LoadCurrentRequestDataCommand = new CommandType();
            LoadCurrentRequestDataCommand.Create(_ => ReloadRequestData(true));
            SwitchGroupingCommand = new CommandType();
            SwitchGroupingCommand.Create(_ => { IsGroupingByCategories = !IsGroupingByCategories; ReloadRequestData(false); });
            ProceedRequestCommand = new CommandType();
            ProceedRequestCommand.Create(_ => ProceedRequest(), _ => RequestIsReadyToProceed);
            RemoveProductCommand = new CommandType();
            RemoveProductCommand.Create(p => RemoveProduct((ProductForRequestView)p));
            RemoveProductsCategoryCommand = new CommandType();
            RemoveProductsCategoryCommand.Create(c => RemoveProductsCategory((CategoryForRequestView)c));
            CategoriesSelectionChangedCommand = new CommandType();
            CategoriesSelectionChangedCommand.Create(_ => { OnPropertyChanged("SuppliersCount"); OnPropertyChanged("ProductsNamesCount"); OnPropertyChanged("ItemsCount"); OnPropertyChanged("TotalSum"); });
        }
    }
}
