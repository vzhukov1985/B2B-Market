using ClientApp_Mobile.Models;
using ClientApp_Mobile.Services;
using Core.DBModels;
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
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    public class CurrentRequestSubPageVM : BaseVM
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
                    return Categories.Where(c => c.IsSelected).SelectMany(c => c.SelectMany(p => p.Orders)).Sum(o => o.OrderQuantity);
                return 0;
            }
        }

        public int ProductsNamesCount
        {
            get
            {
                if (Categories != null)
                    return Categories.Where(c => c.IsSelected).SelectMany(c => c.SelectMany(p => p.Orders)).GroupBy(o => o.ProductId).Count();
                return 0;
            }
        }

        public int SuppliersCount
        {
            get
            {
                if (Categories != null)
                    return Categories.Where(c => c.IsSelected).SelectMany(c => c.SelectMany(p => p.Orders)).GroupBy(o => o.SupplierId).Count();
                return 0;
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

        private List<OfferWithOrder> Orders;
        private List<Product> ProductsFromDb;


        private void ProceedRequest()
        {
            IsBusy = true;
            try
            {
                int LastRequestCode;
                using (MarketDbContext db = new MarketDbContext())
                {
                    LastRequestCode = db.ArchivedRequests.Count() > 0 ? db.ArchivedRequests.Max(r => r.Code) : 0;
                }

                List<ArchivedRequest> requestsToAdd = Categories.Where(c => c.IsSelected).SelectMany(c => c.SelectMany(p => p.Orders)).GroupBy(c => c.Supplier).Select(s => new ArchivedRequest
                {
                    Client = User.Client,
                    ClientId = User.ClientId,
                    SenderName = User.Name,
                    SenderSurname = User.Surname,
                    ItemsQuantity = Orders.Where(o => o.SupplierId == s.Key.Id).Sum(o => o.OrderQuantity),
                    ProductsQuantity = Orders.Where(o => o.SupplierId == s.Key.Id).GroupBy(o => o.ProductId).Count(),
                    TotalPrice = Orders.Where(o => o.SupplierId == s.Key.Id).Sum(o => o.PriceForClient * o.OrderQuantity),
                    ArchivedSupplierId = s.Key.Id,
                    DateTimeSent = DateTime.Now,
                    DeliveryDateTime = DateTime.Today.AddDays(1) + new TimeSpan(10, 0, 0),
                    Comments = "",
                    ArchivedSupplier = new ArchivedSupplier
                    {
                        Id = s.Key.Id,
                        Address = s.Key.Address,
                        Bin = s.Key.BIN,
                        Email = s.Key.Email,
                        FullName = s.Key.FullName,
                        Phone = s.Key.Phone,
                        ShortName = s.Key.Phone
                    },
                    ArchivedRequestsStatuses = new List<ArchivedRequestsStatus>
                {
                        new ArchivedRequestsStatus {ArchivedRequestStatusTypeId = new Guid("ceff6b71-a27c-468b-b9f6-fd0ccc8d6024"), DateTime = DateTime.Now }, //SENT
                        new ArchivedRequestsStatus { ArchivedRequestStatusTypeId = new Guid("3df59a9b-4874-4aa4-83de-545fd0d0e6ec"), DateTime = DateTime.Now.AddSeconds(1) }  //PENDING
                },
                    ArchivedOrders = new List<ArchivedOrder>(Orders.Where(o => o.SupplierId == s.Key.Id).Select(o => new ArchivedOrder
                    {
                        Id = Guid.NewGuid(),
                        OfferId = o.Id,
                        SupplierProductCode = o.SupplierProductCode,
                        Price = o.PriceForClient,
                        QuantityUnit = o.QuantityUnit.ShortName,
                        Quantity = o.OrderQuantity,
                        Remains = o.Remains,
                        ProductName = ProductsFromDb.Where(p => p.Id == o.ProductId).FirstOrDefault().Name,
                        ProductCategory = ProductsFromDb.Where(p => p.Id == o.ProductId).FirstOrDefault().Category.Name,
                        ProductCode = ProductsFromDb.Where(p => p.Id == o.ProductId).FirstOrDefault().Code,
                        ProductVolumeType = ProductsFromDb.Where(p => p.Id == o.ProductId).FirstOrDefault().VolumeType.Name,
                        ProductVolumeUnit = ProductsFromDb.Where(p => p.Id == o.ProductId).FirstOrDefault().VolumeUnit.ShortName,
                        ProductVolume = ProductsFromDb.Where(p => p.Id == o.ProductId).FirstOrDefault().Volume,
                        ProductId = o.ProductId,
                        Product = ProductsFromDb.Where(p => p.Id == o.ProductId).FirstOrDefault()
                    }))
                }).ToList();

                foreach (ArchivedRequest request in requestsToAdd)
                {
                    request.Id = Guid.NewGuid();
                    foreach (ArchivedRequestsStatus status in request.ArchivedRequestsStatuses)
                        status.ArchivedRequestId = request.Id;
                    foreach (ArchivedOrder order in request.ArchivedOrders)
                        order.ArchivedRequestId = request.Id;
                    request.Code = LastRequestCode + 1;
                    LastRequestCode++;
                }

                IsBusy = false;
                Device.BeginInvokeOnMainThread(() => ShellPageService.GotoCurrentRequestConfirmPage(requestsToAdd));
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => ShellDialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
        }

        private async void RemoveProduct(ProductForRequestView selectedProduct)
        {
            string dialogText;
            if (IsGroupingByCategories)
                dialogText = $"Вы действительно хотите удалить \"{selectedProduct.Name}\" от всех поставщиков?";
            else
                dialogText = $"Вы действительно хотите удалить \"{selectedProduct.Name}\" от поставщика \"{selectedProduct.Orders[0].Supplier.FullName}\"?";

            if (await ShellDialogService.ShowOkCancelDialog(dialogText, "Внимание!") == false)
                return;

            await Task.Run(() =>
            {
                List<Guid> OffersToRemoveIds = selectedProduct.Orders.Select(od => od.Id).ToList();

                IsBusy = true;
                try
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.CurrentOrders.RemoveRange(db.CurrentOrders.Where(o => OffersToRemoveIds.Contains(o.OfferId) && o.ClientId == User.ClientId));
                        db.SaveChanges();
                    }
                    foreach (CurrentOrder order in User.Client.CurrentOrders)
                    {
                        if (OffersToRemoveIds.Contains(order.OfferId) && order.ClientId == User.ClientId)
                            User.Client.CurrentOrders.Remove(order);
                    }
                    QueryDb(false);
                    IsBusy = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() => ShellDialogService.ShowConnectionErrorDlg());
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

            if (await ShellDialogService.ShowOkCancelDialog(dialogText, "Внимание!") == false)
                return;

            await Task.Run(() =>
            {
                IsBusy = true;
                try
                {
                    List<Guid> OffersToRemoveIds = selectedCategory.SelectMany(p => p.Orders.Select(o => o.Id)).ToList();

                    using (MarketDbContext db = new MarketDbContext())
                    {
                        db.CurrentOrders.RemoveRange(db.CurrentOrders.Where(o => OffersToRemoveIds.Contains(o.OfferId) && o.ClientId == User.ClientId));
                        db.SaveChanges();
                    }
                    foreach (CurrentOrder order in User.Client.CurrentOrders)
                    {
                        if (OffersToRemoveIds.Contains(order.OfferId) && order.ClientId == User.ClientId)
                            User.Client.CurrentOrders.Remove(order);
                    }
                    QueryDb(false);
                    IsBusy = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() => ShellDialogService.ShowConnectionErrorDlg());
                    IsBusy = false;
                    return;
                }
            });
        }

        public async void QueryDb(bool requeryProductsDB)
        {
            IsBusy = true;
            try
            {
                if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                List<Guid> ContractedSuppliersIds = User.Client.Contracts.Select(p => p.Supplier.Id).ToList();
                List<Guid> FavoriteProductsIds = User.Favorites.Select(f => f.Product.Id).ToList();

                if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                List<Guid> ProductIds = User.Client.CurrentOrders.Select(o => o.Offer.ProductId).Distinct().ToList();

                if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                Orders = User.Client.CurrentOrders.Select(o => new OfferWithOrder
                {
                    Id = o.OfferId,
                    SupplierProductCode = o.Offer.SupplierProductCode,
                    IsActive = o.Offer.IsActive,
                    SupplierId = o.Offer.SupplierId,
                    Supplier = o.Offer.Supplier,
                    ProductId = o.Offer.ProductId,
                    Product = o.Offer.Product,
                    Remains = o.Offer.Remains,
                    QuantityUnitId = o.Offer.QuantityUnitId,
                    QuantityUnit = o.Offer.QuantityUnit,
                    RetailPrice = o.Offer.RetailPrice,
                    DiscountPrice = o.Offer.DiscountPrice,
                    IsOfContractedSupplier = ContractedSuppliersIds.Contains(o.Offer.SupplierId),
                    OrderQuantity = o.Quantity,
                    OrderQuantityBeforeUserChanges = o.Quantity,
                    PriceForClient = ContractedSuppliersIds.Contains(o.Offer.SupplierId) ? o.Offer.DiscountPrice : o.Offer.RetailPrice
                }).ToList();

                if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                if (requeryProductsDB)
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        foreach (OfferWithOrder order in Orders)
                        {
                            if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                            order.Remains = await db.Offers.Where(of => of.Id == order.Id).Select(of => of.Remains).FirstOrDefaultAsync(CTS.Token);

                        }

                        if (CTS.IsCancellationRequested) { IsBusy = false; return; }
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
                             .ToListAsync(CTS.Token);
                    }
                }
                else
                {
                    if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                    ProductsFromDb = ProductsFromDb
                             .Where(p => ProductIds.Contains(p.Id))
                             .OrderBy(p => p.Category.MidCategory.TopCategory.Name)
                             .ThenBy(p => p.Category.Name)
                             .ThenBy(p => p.Name).ToList();
                }

                if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                if (IsGroupingByCategories)
                {

                    Categories = ProductsFromDb
                        .GroupBy(p => p.Category.MidCategory.TopCategory)
                        .Select(c => new CategoryForRequestView(c.Key,
                        ProductsFromDb.Where(p => p.Category.MidCategory.TopCategory.Id == c.Key.Id).Select(p => new ProductForRequestView(p)
                        {
                            Orders = new List<OfferWithOrder>(Orders.Where(o => o.ProductId == p.Id).OrderByDescending(o => ContractedSuppliersIds.Contains(o.SupplierId))),
                            IsOfContractedSupplier = p.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id)),
                            IsFavoriteForUser = FavoriteProductsIds.Contains(p.Id)
                        }))

                        {
                            IsSelected = true,
                        }).ToList();
                }
                else
                {
                    Categories = Orders
                        .GroupBy(c => c.Supplier)
                        .OrderByDescending(c => ContractedSuppliersIds.Contains(c.Key.Id))
                        .ThenBy(c => c.Key.ShortName)
                        .Select(c => new CategoryForRequestView(c.Key,
                            ProductsFromDb.
                            Where(p => p.Offers.Select(o => o.SupplierId).Contains(c.Key.Id) &&
                            Orders.Where(oo => oo.SupplierId == c.Key.Id).Select(oo => oo.ProductId).Contains(p.Id))
                            .Select(p => new ProductForRequestView(p)
                            {
                                Orders = new List<OfferWithOrder>(Orders.Where(o => (o.ProductId == p.Id) && (o.SupplierId == c.Key.Id))),
                                IsOfContractedSupplier = p.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id)),
                                IsFavoriteForUser = FavoriteProductsIds.Contains(p.Id)
                            }))

                        {
                            IsSelected = true,

                        }).ToList();
                    if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                    Device.BeginInvokeOnMainThread(() => ProceedRequestCommand.ChangeCanExecute());
                }

                if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                foreach (var cat in Categories)
                {
                    cat.PropertyChanged += (s, a) => { OnPropertyChanged("SuppliersCount"); OnPropertyChanged("ProductsNamesCount"); OnPropertyChanged("ItemsCount"); OnPropertyChanged("TotalSum"); ProceedRequestCommand.ChangeCanExecute(); };
                }
                IsBusy = false;
            }
            catch (OperationCanceledException)
            {
                IsBusy = false;
                return;
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => ShellDialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
        }

        public Command AddRemoveProductToFavouritesCommand { get; }
        public Command ShowProductCommand { get; }
        public Command SwitchGroupingCommand { get; }
        public Command ProceedRequestCommand { get; }

        public Command RemoveProductCommand { get; }
        public Command RemoveProductsCategoryCommand { get; }


        public CurrentRequestSubPageVM()
        {
            User = UserService.CurrentUser;

            AddRemoveProductToFavouritesCommand = new Command(p => Task.Run(() => MarketDbContext.AddRemoveProductToFavourites((Product)p, UserService.CurrentUser)));
            ShowProductCommand = new Command(p => ShellPageService.GotoProductPage(p is OfferWithOrder order ? order.Product : ((Product)p)));

            SwitchGroupingCommand = new Command(_ => Task.Run(() => { IsGroupingByCategories = !IsGroupingByCategories; QueryDb(false); }));
            ProceedRequestCommand = new Command(_ => Task.Run(() => ProceedRequest()), _ => CheckIfRequestIsReadyToProceed());
            RemoveProductCommand = new Command(p => RemoveProduct((ProductForRequestView)p));
            RemoveProductsCategoryCommand = new Command(c => RemoveProductsCategory((CategoryForRequestView)c));
        }
    }
}
