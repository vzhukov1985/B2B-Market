using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    class CurrentRequestSubPageVM : BaseVM
    {
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

        private List<CategoryForRequestView> _categories;
        public List<CategoryForRequestView> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                OnPropertyChanged("Categories");
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

        public decimal TotalSum
        {
            get
            {
                if (Categories != null)
                    return Categories.Where(c => c.IsSelected).Sum(c => c.Sum(p => p.Orders.Sum(o => o.PriceForClient * o.OrderQuantity)));
                return 0;
            }
        }

        public bool CheckIfRequestIsReadyToProceed()
        {
            if (Categories != null)
            {
                var a = Categories.Where(c => c.IsSelected).SelectMany(c => c.Select(p => p.OrderAndRemainsState));

                return a.All(st => st == ProductOrderAndRemainsState.Ok) && User.IsAdmin && a.Count() > 0;
            }
            return false;
        }

        private List<Guid> ContractedSuppliersIds;
        private List<OrderFromDbView> AllCurrentOrders;


        public Command AddRemoveProductToFavouritesCommand { get; }
        public Command ShowProductCommand { get; }
        public Command SwitchGroupingCommand { get; }
        public Command ProceedRequestCommand { get; }

        public Command RemoveProductCommand { get; }
        public Command RemoveProductsCategoryCommand { get; }


        public CurrentRequestSubPageVM()
        {
            User = AppSettings.CurrentUser;
            ContractedSuppliersIds = AppSettings.CurrentUser.Client.ContractedSuppliersIDs;

            AddRemoveProductToFavouritesCommand = new Command<ProductForRequestView>(p => Task.Run(() => AddRemoveProductToFavorites(p)));
            ShowProductCommand = new Command(p => ShellPageService.GotoProductPage(GetProductByOfferId(p is OrderForRequestView order ? order.OfferId : ((ProductForRequestView)p).Orders.FirstOrDefault().OfferId)));

            SwitchGroupingCommand = new Command(_ => Task.Run(() => { IsGroupingByCategories = !IsGroupingByCategories; QueryDb(); }));
            ProceedRequestCommand = new Command(_ => Task.Run(() => ProceedRequest()), _ => CheckIfRequestIsReadyToProceed());
            RemoveProductCommand = new Command(p => RemoveProduct((ProductForRequestView)p));
            RemoveProductsCategoryCommand = new Command(c => RemoveProductsCategory((CategoryForRequestView)c));
        }

        public async void QueryDb()
        {
            IsBusy = true;

            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    AllCurrentOrders = await db.CurrentOrders
                                         .Where(co => co.ClientId == AppSettings.CurrentUser.ClientId)
                                         .Select(co => new OrderFromDbView
                                         {
                                             ProductId = co.Offer.Product.Id,
                                             ProductName = co.Offer.Product.Name,
                                             ProductPictureUri = co.Offer.Product.PictureUri,
                                             ProductTopCategoryId = co.Offer.Product.Category.MidCategory.TopCategoryId,
                                             ProductTopCategoryName = co.Offer.Product.Category.MidCategory.TopCategory.Name,
                                             ProductCategoryName = co.Offer.Product.Category.Name,
                                             VolumeType = co.Offer.Product.VolumeType.Name,
                                             Volume = co.Offer.Product.Volume,
                                             VolumeUnit = co.Offer.Product.VolumeUnit.ShortName,
                                             OfferId = co.OfferId,
                                             IsActive = co.Offer.IsActive,
                                             IsSupplierActive = co.Offer.Supplier.IsActive,
                                             OrderQuantity = co.Quantity,
                                             PriceForClient = ContractedSuppliersIds.Contains(co.Offer.SupplierId) ? co.Offer.DiscountPrice : co.Offer.RetailPrice,
                                             QuantityUnit = co.Offer.QuantityUnit.ShortName,
                                             Remains = co.Offer.Remains,
                                             SupplierId = co.Offer.SupplierId,
                                             SupplierShortName = co.Offer.Supplier.ShortName,
                                             SupplierCountry = co.Offer.Supplier.Country,
                                             SupplierCity = co.Offer.Supplier.City,
                                             SupplierAddress = co.Offer.Supplier.Address,
                                             SupplierBin = co.Offer.Supplier.Bin,
                                             SupplierEmail = co.Offer.Supplier.Email,
                                             SupplierFullName = co.Offer.Supplier.FullName,
                                             SupplierPhone = co.Offer.Supplier.Phone,
                                             ProductCode = co.Offer.Product.Code,
                                             SupplierProductCode = co.Offer.SupplierProductCode,
                                             SupplierFTPUser = co.Offer.Supplier.FTPUser
                                         }).ToListAsync(CTS.Token);
                }
            }
            catch (OperationCanceledException)
            {
                IsBusy = false;
                return;
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }

            foreach (var order in AllCurrentOrders)
            {
                order.IsFavoriteForUser = AppSettings.CurrentUser.Favorites.Select(f => f.ProductId).Contains(order.ProductId);
            }

            if (IsGroupingByCategories)
            {
                Categories = AllCurrentOrders
                                 .GroupBy(o => o.ProductTopCategoryId)
                                 .Select(gtc =>
                                     new CategoryForRequestView(gtc.FirstOrDefault().ProductTopCategoryName,
                                                                true,
                                                                GetProductsListFromGroupedOrders(gtc).OrderBy(p => p.Name)))
                                 .OrderBy(c => c.Name).ToList();
            }
            else
            {
                Categories = AllCurrentOrders
                                 .GroupBy(o => o.SupplierId)
                                 .Select(gs =>
                                    new CategoryForRequestView(gs.FirstOrDefault().SupplierShortName,
                                                               false,
                                                               GetProductsListFromGroupedOrders(gs).OrderBy(p => p.Name)))
                                 .OrderBy(c => c.IsContractedCategory)
                                 .ThenBy(c => c.Name).ToList();
            }


            if (CTS.IsCancellationRequested) { IsBusy = false; return; }
            Device.BeginInvokeOnMainThread(() => ProceedRequestCommand.ChangeCanExecute());

            if (CTS.IsCancellationRequested) { IsBusy = false; return; }
            foreach (var cat in Categories)
            {
                cat.PropertyChanged += (s, a) =>
                {
                    if (a.PropertyName == "IsSelected" && AllCurrentOrders != null)
                    {
                        foreach (var product in (CategoryForRequestView)s)
                        {
                            foreach (var order in product.Orders)
                            {
                                AllCurrentOrders.Where(o => o.OfferId == order.OfferId).FirstOrDefault().IsSelected = ((CategoryForRequestView)s).IsSelected;
                            }
                        }
                    }
                    OnPropertyChanged("TotalSum");
                    ProceedRequestCommand.ChangeCanExecute();
                };
            }
            IsBusy = false;
        }

        private void ProceedRequest()
        {
            IsBusy = true;
            int LastRequestCode;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    LastRequestCode = db.ArchivedRequests.Count() > 0 ? db.ArchivedRequests.Max(r => r.Code) : 0;
                }
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }

            List<RequestForConfirmation> requestsToAdd = AllCurrentOrders.Where(o => o.IsSelected == true).GroupBy(o => o.SupplierId).Select(so => new RequestForConfirmation
            {
                ArchivedClientId = AppSettings.CurrentUser.ClientId,
                Client = User.Client,
                ClientId = User.ClientId,
                SupplierId = so.Key,
                SenderName = User.Name,
                SenderSurname = User.Surname,
                ItemsQuantity = (int) so.Select(o => o.OrderQuantity).Sum(),
                ProductsQuantity = so.Select(o => o.ProductId).Distinct().Count(),
                TotalPrice = Categories.Where(c => c.IsSelected).Sum(c => c.Sum(p => p.Orders.Sum(o => o.PriceForClient * o.OrderQuantity))),
                ArchivedSupplierId = so.Key,
                DateTimeSent = DateTime.Now,
                DeliveryDateTime = DateTime.Today.AddDays(1) + new TimeSpan(10, 0, 0),
                Comments = "",
                FTPSupplierFolder = so.FirstOrDefault().SupplierFTPUser,
                ArchivedSupplier = new ArchivedSupplier
                {
                    Id = so.FirstOrDefault().SupplierId,
                    ShortName = so.FirstOrDefault().SupplierShortName,
                    FullName = so.FirstOrDefault().SupplierFullName,
                    Bin = so.FirstOrDefault().SupplierBin,
                    Country = so.FirstOrDefault().SupplierCountry,
                    City = so.FirstOrDefault().SupplierCity,
                    Address = so.FirstOrDefault().SupplierAddress,
                    Phone = so.FirstOrDefault().SupplierPhone,
                    Email = so.FirstOrDefault().SupplierEmail,
                },
                ArchivedClient = new ArchivedClient
                {
                    Id = User.Client.Id,
                    ShortName = User.Client.ShortName,
                    FullName = User.Client.FullName,
                    Bin = User.Client.Bin,
                    Country = User.Client.Country,
                    City = User.Client.City,
                    Address = User.Client.Address,
                    Phone = User.Client.Phone,
                    Email = User.Client.Email
                },
                ArchivedRequestsStatuses = new List<ArchivedRequestsStatus>
                {
                    new ArchivedRequestsStatus {ArchivedRequestStatusTypeId = AppSettings.ArchivedOrderStatuses["SENT"], DateTime = DateTime.Now }, //SENT
                    new ArchivedRequestsStatus {ArchivedRequestStatusTypeId = AppSettings.ArchivedOrderStatuses["PENDING"], DateTime = DateTime.Now.AddSeconds(1) }  //PENDING
                },
                OrdersToConfirm = so.Select(o => new OrderOfRequestForConfirmation
                {
                    OfferId = o.OfferId,
                    Remains = o.Remains,
                    Id = Guid.NewGuid(),
                    SupplierProductCode = o.SupplierProductCode,
                    Price = o.PriceForClient,
                    QuantityUnit = o.QuantityUnit,
                    Quantity = o.OrderQuantity,
                    ProductName = o.ProductName,
                    ProductCategory = o.ProductCategoryName,
                    ProductCode = o.ProductCode,
                    ProductVolumeType = o.VolumeType,
                    ProductVolumeUnit = o.VolumeUnit,
                    ProductVolume = o.Volume,
                    Product = new ProductForConfirmRequestView
                    {
                        Name = o.ProductName,
                        VolumeType = o.VolumeType,
                        Volume = o.Volume,
                        VolumeUnit = o.VolumeUnit,
                        CategoryName = o.ProductCategoryName,
                        PictureUri = o.ProductPictureUri
                    },
                }).ToList()
            }).ToList();

            foreach (var request in requestsToAdd)
            {
                request.Id = Guid.NewGuid();
                foreach (var status in request.ArchivedRequestsStatuses)
                    status.ArchivedRequestId = request.Id;
                foreach (var order in request.OrdersToConfirm)
                    order.ArchivedRequestId = request.Id;
                request.Code = LastRequestCode + 1;
                LastRequestCode++;
            }
            Device.BeginInvokeOnMainThread(() => ShellPageService.GotoCurrentRequestConfirmPage(requestsToAdd));
            IsBusy = false;
        }

        private async void RemoveProduct(ProductForRequestView selectedProduct)
        {
            string dialogText;
            if (IsGroupingByCategories)
                dialogText = $"Вы действительно хотите удалить \"{selectedProduct.Name}\" от всех поставщиков?";
            else
                dialogText = $"Вы действительно хотите удалить \"{selectedProduct.Name}\" от поставщика \"{selectedProduct.Orders[0].SupplierName}\"?";

            if (await DialogService.ShowOkCancelDialog(dialogText, "Внимание!") == false)
                return;

            await Task.Run(() =>
            {
                List<Guid> OffersToRemoveIds = selectedProduct.Orders.Select(od => od.OfferId).ToList();

                IsBusy = true;
                try
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.CurrentOrders.RemoveRange(db.CurrentOrders.Where(o => OffersToRemoveIds.Contains(o.OfferId) && o.ClientId == User.ClientId));
                        db.SaveChanges();
                    }

                    User.Client.CurrentOrders.RemoveAll(o => OffersToRemoveIds.Contains(o.OfferId) && o.ClientId == User.ClientId);

                    QueryDb();
                    IsBusy = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                    IsBusy = false;
                    return;
                }
            });
        }


        private async void RemoveProductsCategory(CategoryForRequestView selectedCategory)
        {
            string dialogText;
            if (IsGroupingByCategories)
                dialogText = "Вы действительно хотите удалить все выбранные товары в категории \"" + selectedCategory.Name + "\"?";
            else
                dialogText = "Вы действительно хотите удалить все выбранные товары от поставщика \"" + selectedCategory.Name + "\"?";

            if (await DialogService.ShowOkCancelDialog(dialogText, "Внимание!") == false)
                return;

            await Task.Run(() =>
            {
                IsBusy = true;
                try
                {
                    List<Guid> OffersToRemoveIds = selectedCategory.SelectMany(p => p.Orders.Select(o => o.OfferId)).ToList();

                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.CurrentOrders.RemoveRange(db.CurrentOrders.Where(o => OffersToRemoveIds.Contains(o.OfferId) && o.ClientId == User.ClientId));
                        db.SaveChanges();
                    }

                    User.Client.CurrentOrders.RemoveAll(o => OffersToRemoveIds.Contains(o.OfferId) && o.ClientId == User.ClientId);


                    QueryDb();
                    IsBusy = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                    IsBusy = false;
                    return;
                }
            });
        }

        private void AddRemoveProductToFavorites(ProductForRequestView p)
        {
            MarketDbContext.AddRemoveProductToFavourites(new Product { Id = p.Id, IsFavoriteForUser = p.IsFavoriteForUser }, AppSettings.CurrentUser);
            foreach (var product in Categories.SelectMany(c => c))
            {
                if (product.Id == p.Id)
                    product.IsFavoriteForUser = !product.IsFavoriteForUser;
            }
        }

        private IEnumerable<ProductForRequestView> GetProductsListFromGroupedOrders(IGrouping<Guid, OrderFromDbView> group)
        {
            return group.GroupBy(gp => gp.ProductId).Select(gp => new ProductForRequestView
            {
                CategoryName = gp.FirstOrDefault().ProductCategoryName,
                Id = gp.Key,
                IsFavoriteForUser = gp.FirstOrDefault().IsFavoriteForUser,
                IsOfContractedSupplier = ContractedSuppliersIds.Contains(gp.FirstOrDefault().SupplierId),
                Name = gp.FirstOrDefault().ProductName,
                PictureUri = gp.FirstOrDefault().ProductPictureUri,
                Volume = gp.FirstOrDefault().Volume,
                VolumeType = gp.FirstOrDefault().VolumeType,
                VolumeUnit = gp.FirstOrDefault().VolumeUnit,
                Orders = gp.Select(o => new OrderForRequestView
                {
                    IsActive = o.IsActive,
                    IsOfContractedSupplier = ContractedSuppliersIds.Contains(o.SupplierId),
                    IsSupplierActive = o.IsSupplierActive,
                    OfferId = o.OfferId,
                    OrderQuantity = o.OrderQuantity,
                    PriceForClient = o.PriceForClient,
                    QuantityUnit = o.QuantityUnit,
                    Remains = o.Remains,
                    SupplierId = o.SupplierId,
                    SupplierName = o.SupplierShortName,
                }).OrderByDescending(o => o.IsOfContractedSupplier).ThenBy(o => o.SupplierName).ToList()
            });
        }


        private Product GetProductByOfferId(Guid offerId)
        {
            OrderFromDbView order = AllCurrentOrders.Where(o => o.OfferId == offerId).FirstOrDefault();
            return new Product
            {
                Id = order.ProductId,
                Name = order.ProductName,
                Category = new ProductCategory { Name = order.ProductCategoryName },
                Code = order.ProductCode,
                IsFavoriteForUser = AppSettings.CurrentUser.Favorites.Select(f => f.ProductId).Contains(order.ProductId),
                IsOfContractedSupplier = order.IsOfContractedSupplier,
                Volume = order.Volume,
                VolumeType = new VolumeType { Name = order.VolumeType },
                VolumeUnit = new VolumeUnit { ShortName = order.VolumeUnit }
            };
        }

    }




    class CategoryForRequestView : List<ProductForRequestView>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
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
                return this.Sum(p => p.Orders.Sum(o => o.PriceForClient * o.OrderQuantity));
            }
        }

        public bool? IsContractedCategory
        {
            get
            {
                if (isGroupingeByCategories)
                {
                    return null;
                }
                else
                {
                    return this.All(p => p.IsOfContractedSupplier == true);
                }
            }
        }

        private readonly bool isGroupingeByCategories;
        public CategoryForRequestView(string categoryName, bool isGroupingByCategories, IEnumerable<ProductForRequestView> products) : base(products)
        {
            Name = categoryName;
            isGroupingeByCategories = isGroupingByCategories;
            IsSelected = true;
        }

    }

    class ProductForRequestView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Uri PictureUri { get; set; }
        public bool IsOfContractedSupplier { get; set; }
        private bool _isFavoriteForUser;
        public bool IsFavoriteForUser
        {
            get { return _isFavoriteForUser; }
            set
            {
                _isFavoriteForUser = value;
                OnPropertyChanged("IsFavoriteForUser");
            }
        }
        public string CategoryName { get; set; }
        public string VolumeType { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; }

        private List<OrderForRequestView> _orders;
        public List<OrderForRequestView> Orders
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
                if (Orders.All(o => o.Remains == 0 || o.IsSupplierActive == false || o.IsActive == false))
                {
                    if (Orders.Count == 1)
                        return ProductOrderAndRemainsState.OneSupplierNullRemains;
                    else
                        return ProductOrderAndRemainsState.AllSuppliersNullRemains;
                }

                if (Orders.Any(o => o.OrderQuantity > o.Remains || o.IsSupplierActive == false || o.IsActive == false))
                {
                    if (Orders.Count == 1)
                        return ProductOrderAndRemainsState.OneSupplierLessRemains;
                    else
                        return ProductOrderAndRemainsState.OneOfSuppliersLessRemains;
                }
                return ProductOrderAndRemainsState.Ok;
            }
        }
    }
    enum ProductOrderAndRemainsState
    {
        Ok,
        OneSupplierNullRemains,
        AllSuppliersNullRemains,
        OneSupplierLessRemains,
        OneOfSuppliersLessRemains
    }

    class OrderForRequestView
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public Guid OfferId { get; set; }

        public decimal Remains { get; set; }

        public bool IsActive { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; }
        public bool IsSupplierActive { get; set; }

        public bool IsOfContractedSupplier { get; set; }

        public decimal PriceForClient { get; set; }

        private decimal _orderQuantity;
        public decimal OrderQuantity
        {
            get { return _orderQuantity; }
            set
            {
                _orderQuantity = value;
                OnPropertyChanged("OrderQuantity");
            }
        }

        public string QuantityUnit { get; set; }
    }

    class OrderFromDbView
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public Uri ProductPictureUri { get; set; }
        public string ProductCategoryName { get; set; }
        public int ProductCode { get; set; }
        public string VolumeType { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; }
        public bool IsFavoriteForUser { get; set; }

        public Guid SupplierId { get; set; }
        public string SupplierShortName { get; set; }
        public string SupplierFullName { get; set; }
        public string SupplierBin { get; set; }
        public string SupplierCountry { get; set; }
        public string SupplierCity { get; set; }
        public string SupplierAddress { get; set; }
        public string SupplierPhone { get; set; }
        public string SupplierEmail { get; set; }
        public string SupplierFTPUser { get; set; }
        public bool IsSupplierActive { get; set; }

        public Guid ProductTopCategoryId { get; set; }
        public string ProductTopCategoryName { get; set; }

        public Guid OfferId { get; set; }
        public string SupplierProductCode { get; set; }
        public decimal Remains { get; set; }
        public bool IsActive { get; set; }
        public decimal PriceForClient { get; set; }
        public decimal OrderQuantity { get; set; }
        public string QuantityUnit { get; set; }

        public bool IsOfContractedSupplier { get; set; }
        public bool IsSelected { get; set; }
    }
}
