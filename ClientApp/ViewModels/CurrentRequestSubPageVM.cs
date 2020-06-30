using ClientApp.Services;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
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
    public class ProductForRequestView : Product
    {
        private ObservableCollection<OrderByUser> _orders;
        public ObservableCollection<OrderByUser> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged("Orders");
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

        private int _itemsCount;
        public int ItemsCount
        {
            get { return _itemsCount; }
            set
            {
                _itemsCount = value;
                OnPropertyChanged("ItemsCount");
            }
        }

        private int _productNamesCount;
        public int ProductsNamesCount
        {
            get { return _productNamesCount; }
            set
            {
                _productNamesCount = value;
                OnPropertyChanged("ProductsNamesCount");
            }
        }

        private int _suppliersCount;
        public int SuppliersCount
        {
            get { return _suppliersCount; }
            set
            {
                _suppliersCount = value;
                OnPropertyChanged("SuppliersCount");
            }
        }

        private decimal _totalSum;
        public decimal TotalSum
        {
            get { return _totalSum; }
            set
            {
                _totalSum = value;
                OnPropertyChanged("TotalSum");
            }
        }

        private async void QueryDB()
        {
            List<Guid> ContractedSuppliersIds = User.Client.Contracts.Select(p => p.Supplier.Id).ToList();
            List<Guid> FavoriteProductsIds = User.FavoriteProducts.Select(f => f.Product.Id).ToList();

            Orders = User.Client.CurrentOrders.Select(o => new OrderByUser
            {
                Id = o.OfferId,
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

            if (IsGroupingByCategories)
            {
                Categories = new ObservableCollection<CategoryForRequestView>(ProductsFromDb.GroupBy(p => p.Category.MidCategory.TopCategory).Select(c => new CategoryForRequestView(c.Key)
                {
                    Products = new ObservableCollection<ProductForRequestView>(ProductsFromDb.Where(p => p.Category.MidCategory.TopCategory.Id == c.Key.Id).Select(p => new ProductForRequestView(p)
                    {
                        Orders = new ObservableCollection<OrderByUser>(Orders.Where(o => o.ProductId == p.Id).OrderByDescending(o => ContractedSuppliersIds.Contains(o.SupplierId))),
                        IsOfContractedSupplier = p.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id)),
                        IsFavoriteForUser = FavoriteProductsIds.Contains(p.Id)
                    }))
                }));
            }
            else
            {
                Categories = new ObservableCollection<CategoryForRequestView>(Orders.GroupBy(c => c.Supplier).OrderByDescending(c => ContractedSuppliersIds.Contains(c.Key.Id)).ThenBy(c => c.Key.ShortName).Select(c => new CategoryForRequestView(c.Key)
                {
                    Products = new ObservableCollection<ProductForRequestView>(ProductsFromDb.
                    Where(p => p.Offers.Select(o => o.SupplierId).Contains(c.Key.Id) &&
                    Orders.Where(oo => oo.SupplierId == c.Key.Id).Select(oo => oo.ProductId).Contains(p.Id))
                    .Select(p => new ProductForRequestView(p)
                    {
                        Orders = new ObservableCollection<OrderByUser>(Orders.Where(o => (o.ProductId == p.Id) && (o.SupplierId == c.Key.Id))),
                        IsOfContractedSupplier = p.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id)),
                        IsFavoriteForUser = FavoriteProductsIds.Contains(p.Id)
                    }))
                }));
            }

            ItemsCount = Orders.Sum(o => o.OrderQuantity);
            ProductsNamesCount = Orders.GroupBy(o => o.ProductId).Count();
            SuppliersCount = Orders.GroupBy(o => o.SupplierId).Count();
            TotalSum = Orders.Sum(o => o.PriceForClient * o.OrderQuantity);
        }

        private void ProceedRequest()
        {
            List<ArchivedRequest> requestsToAdd = Orders.GroupBy(c => c.Supplier).Select(s => new ArchivedRequest
            {
                Id = new Guid(),
                ClientId = User.ClientId,
                SenderName = User.Name,
                SenderSurName = User.SurName,
                ItemsQuantity = Orders.Where(o => o.SupplierId == s.Key.Id).Sum(o => o.OrderQuantity),
                ProductsQuantity = Orders.Where(o => o.SupplierId == s.Key.Id).GroupBy(o => o.ProductId).Count(),
                TotalPrice = Orders.Where(o => o.SupplierId == s.Key.Id).Sum(o => o.PriceForClient * o.OrderQuantity),
                ArchivedSupplierId = s.Key.Id,
                DateTimeSent = DateTime.Now,
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
                    Price = o.PriceForClient,
                    QuantityUnit = o.QuantityUnit.ShortName,
                    Quantity = o.OrderQuantity,
                    ProductId = o.ProductId,
                }))
            }).ToList();

            using (MarketDbContext db = new MarketDbContext())
            {
                var AvailableArchivedSuppliersIds = db.ArchivedSuppliers.Select(s => s.Id);

                foreach (ArchivedRequest request in requestsToAdd)
                {
                    if (AvailableArchivedSuppliersIds.Contains(request.ArchivedSupplierId))
                        db.Entry(request.ArchivedSupplier).State = EntityState.Unchanged;

                    foreach (ArchivedOrder order in request.Orders)
                        order.ArchivedRequestId = request.Id;

                }
                db.ArchivedRequests.AddRange(requestsToAdd);
                db.CurrentOrders.RemoveRange(User.Client.CurrentOrders);
                db.SaveChanges();

                User.Client.CurrentOrders = new ObservableCollection<CurrentOrder>();
                Orders = new List<OrderByUser>();
                Categories = new ObservableCollection<CategoryForRequestView>();
                ItemsCount = ProductsNamesCount = SuppliersCount = 0;
                TotalSum = 0;
            }


        }

        private List<OrderByUser> Orders;
        private List<Product> ProductsFromDb;

        public CommandType ShowSearchSubPageCommand { get; }
        public CommandType AddRemoveProductToFavouritesCommand { get; }
        public CommandType ShowProductCommand { get; }
        public CommandType NavigationBackCommand { get; }
        public CommandType LoadCurrentRequestDataCommand { get; }
        public CommandType SwitchGroupingCommand { get; }
        public CommandType ProceedRequestCommand { get; set; }


        public CurrentRequestSubPageVM(ClientUser user, IPageService pageService)
        {
            User = user;
            PageService = pageService;

            ShowSearchSubPageCommand = new CommandType();
            ShowSearchSubPageCommand.Create(_ => PageService.ShowSearchSubPage(User));
            AddRemoveProductToFavouritesCommand = new CommandType();
            AddRemoveProductToFavouritesCommand.Create(p => MarketDbContext.AddRemoveProductToFavourites((Product)p, User));
            ShowProductCommand = new CommandType();
            ShowProductCommand.Create(p => PageService.ShowProductSubPage(User, (Product)p));
            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());
            LoadCurrentRequestDataCommand = new CommandType();
            LoadCurrentRequestDataCommand.Create(_ => QueryDB());
            SwitchGroupingCommand = new CommandType();
            SwitchGroupingCommand.Create(_ => { IsGroupingByCategories = !IsGroupingByCategories; QueryDB(); });
            ProceedRequestCommand = new CommandType();
            ProceedRequestCommand.Create(_ => ProceedRequest(), _ => User.IsAdmin && (Orders.Count > 0));
        }
    }
}
