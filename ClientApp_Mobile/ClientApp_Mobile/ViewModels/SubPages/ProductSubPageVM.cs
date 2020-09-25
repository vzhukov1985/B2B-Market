using ClientApp_Mobile.Models;
using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    class ProductSubPageVM : BaseVM
    {
        private int _extraPropsCVHeight;
        public int ExtraPropsCVHeight
        {
            get { return _extraPropsCVHeight; }
            set
            {
                _extraPropsCVHeight = value;
                OnPropertyChanged("ExtraPropsCVHeight");
            }
        }

        private int _offersCVHeight;
        public int OffersCVHeight
        {
            get { return _offersCVHeight; }
            set
            {
                _offersCVHeight = value;
                OnPropertyChanged("OffersCVHeight");
            }
        }



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

        private Product _product;
        public Product Product
        {
            get { return _product; }
            set
            {
                _product = value;
                OnPropertyChanged("Product");
            }
        }

        private List<OfferWithOrderView> _offersWithOrders;
        public List<OfferWithOrderView> OffersWithOrders
        {
            get { return _offersWithOrders; }
            set
            {
                _offersWithOrders = value;
                OnPropertyChanged("OffersWithOrders");
            }
        }

        public bool AreChangesWereMade { get; set; }

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

        private List<CurrentOrder> CurrentRequestOrders;

        private void ProcessChanges()
        {
            AreChangesWereMade = false;
            foreach (var offer in OffersWithOrders)
            {
                if (offer.OrderQuantity > offer.Remains)
                    offer.OrderQuantity = offer.Remains;
                if (offer.OrderQuantity < 0 || offer.IsActive == false || offer.IsSupplierActive == false)
                    offer.OrderQuantity = 0;

                if (offer.IsQuantityWasChanged)
                    AreChangesWereMade = true;
            }

            TotalSum = OffersWithOrders.Sum(o => o.OrderQuantity * o.PriceForClient);
            UpdateCurrentRequestCommand.ChangeCanExecute();
            DecrementOrderCommand.ChangeCanExecute();
            IncrementOrderCommand.ChangeCanExecute();
        }

        private void UpdateCurrentRequest()
        {
            IsBusy = true;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    foreach (var offer in OffersWithOrders)
                    {
                        if (offer.IsQuantityWasChanged)
                        {
                            CurrentOrder currentRequestOrder = CurrentRequestOrders.Where(o => o.OfferId == offer.Id).FirstOrDefault();

                            if (currentRequestOrder == null)
                            {
                                CurrentOrder newRequestOrder = new CurrentOrder
                                {
                                    ClientId = User.ClientId,
                                    OfferId = offer.Id,
                                    Quantity = offer.OrderQuantity
                                };
                                db.CurrentOrders.Add(CurrentOrder.CloneForDB(newRequestOrder));
                                CurrentRequestOrders.Add(newRequestOrder);
                            }
                            else
                            {
                                if (offer.OrderQuantity == 0)
                                {
                                    db.CurrentOrders.Remove(CurrentOrder.CloneForDB(currentRequestOrder));
                                    CurrentRequestOrders.Remove(currentRequestOrder);
                                }
                                else
                                {

                                    currentRequestOrder.Quantity = offer.OrderQuantity;
                                    db.CurrentOrders.Update(CurrentOrder.CloneForDB(currentRequestOrder));
                                }
                            }
                            Device.BeginInvokeOnMainThread(() => { offer.OrderQuantityBeforeUserChanges = offer.OrderQuantity; });
                        }
                    }
                    db.SaveChanges();
                }

                Device.BeginInvokeOnMainThread(() => ProcessChanges());
                AreChangesWereMade = false;
                IsBusy = false;
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
        }

        private async void GoBack()
        {
            if (AreChangesWereMade)
            {
                if (await DialogService.ShowOkCancelDialog("Внимание! Изменения, которые вы сделали, не сохранены и будут сброшены при переходе с этой страницы", "Внимание!") == false)
                    return;
            }
            ShellPageService.GoBack();
        }

        public async void QueryDb()
        {
            IsBusy = true;
            List<Offer> offersFromDb;

            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    Product.ExtraProperties = new ObservableCollection<ProductExtraProperty>(await db.ProductExtraProperties
                                                                                                     .AsNoTracking()
                                                                                                     .Where(pep => pep.ProductId == Product.Id)
                                                                                                     .Select(pep => new ProductExtraProperty
                                                                                                     {
                                                                                                         PropertyType = new ProductExtraPropertyType { Name = pep.PropertyType.Name },
                                                                                                         Value = pep.Value
                                                                                                     })
                                                                                                     .ToListAsync(CTS.Token));
                    Product.Description = new ProductDescription { Text = await db.ProductDescriptions.Where(pd => pd.ProductId == Product.Id).Select(pd => pd.Text).FirstOrDefaultAsync(CTS.Token) };

                    offersFromDb = await db.Offers
                                                       .Where(o => o.ProductId == Product.Id)
                                                       .Select(o => new Offer
                                                       {
                                                           Id = o.Id,
                                                           DiscountPrice = o.DiscountPrice,
                                                           QuantityUnit = new QuantityUnit { ShortName = o.QuantityUnit.ShortName },
                                                           Remains = o.Remains,
                                                           RetailPrice = o.RetailPrice,
                                                           Supplier = new Supplier { Id = o.Supplier.Id, ShortName = o.Supplier.ShortName, IsActive = o.Supplier.IsActive },
                                                           IsActive = o.IsActive
                                                       })
                                                       .ToListAsync(CTS.Token);
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

            List<Guid> ContractedSupplierIds = UserService.CurrentUser.Client.Contracts.Select(c => c.SupplierId).ToList();

            OffersWithOrders = offersFromDb
                .Where(odb => ((odb.Supplier.IsActive == true) && (odb.IsActive == true) && (odb.Remains > 0)) || (CurrentRequestOrders.Where(oo => oo.OfferId == odb.Id).Select(oo => oo.Quantity).FirstOrDefault() > 0))
                .Select(odb => new OfferWithOrderView
                {
                    Id = odb.Id,
                    IsActive = odb.IsActive,
                    IsOfContractedSupplier = ContractedSupplierIds.Contains(odb.Supplier.Id),
                    IsSupplierActive = odb.Supplier.IsActive,
                    SupplierName = odb.Supplier.ShortName,
                    OrderQuantity = CurrentRequestOrders.Where(o => o.OfferId == odb.Id).Select(o => o.Quantity).FirstOrDefault(),
                    QuantityUnit = odb.QuantityUnit.ShortName,
                    OrderQuantityBeforeUserChanges = CurrentRequestOrders.Where(o => o.OfferId == odb.Id).Select(o => o.Quantity).FirstOrDefault(),
                    PriceForClient = ContractedSupplierIds.Contains(odb.SupplierId) ? odb.DiscountPrice : odb.RetailPrice,
                    Remains = odb.Remains
                }).OrderByDescending(oo => oo.IsOfContractedSupplier).ThenBy(oo => oo.SupplierName).ToList();

            foreach (var order in OffersWithOrders)
            {
                if (CTS.IsCancellationRequested) { IsBusy = false; return; }
                order.PropertyChanged += (s, a) => { if (a.PropertyName == "OrderQuantity") ProcessChanges(); };
            }

            ExtraPropsCVHeight = Product.ExtraProperties.Count * 18; //FontSize+5
            OffersCVHeight = OffersWithOrders.Count * 60 + 1;
            AreChangesWereMade = false;

            IsBusy = false;
        }

        public void AddRemoveProductToFavorites()
        {
            try
            {
                MarketDbContext.AddRemoveProductToFavourites(new Product { Id = Product.Id, IsFavoriteForUser = Product.IsFavoriteForUser }, UserService.CurrentUser);
                Product.IsFavoriteForUser = !Product.IsFavoriteForUser;
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
        }

        public Command AddRemoveProductToFavouritesCommand { get; }
        public Command IncrementOrderCommand { get; }
        public Command DecrementOrderCommand { get; }
        public Command ChangesInOrderAreMadeCommand { get; }
        public Command ShowProductPictureCommand { get; }

        public Command UpdateCurrentRequestCommand { get; }
        public Command GoBackCommand { get; }

        public ProductSubPageVM(Product product)
        {
            User = UserService.CurrentUser;
            Product = product;
            CurrentRequestOrders = User.Client.CurrentOrders;

            AddRemoveProductToFavouritesCommand = new Command(_ => Task.Run(() => AddRemoveProductToFavorites()));
            IncrementOrderCommand = new Command<OfferWithOrderView>(o => o.OrderQuantity++, o => o == null ? false : o.OrderQuantity < o.Remains);
            DecrementOrderCommand = new Command<OfferWithOrderView>(o => o.OrderQuantity--, o => o == null ? false : o.OrderQuantity > 0);
            ChangesInOrderAreMadeCommand = new Command(_ => ProcessChanges());
            ShowProductPictureCommand = new Command(_ => ShellPageService.GotoProductPicturePage(Product.Id.ToString(), Product.Name));
            UpdateCurrentRequestCommand = new Command(_ => Task.Run(() => UpdateCurrentRequest()), _ => AreChangesWereMade);
            GoBackCommand = new Command(_ => GoBack());
        }
    }



    class OfferWithOrderView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public Guid Id { get; set; }

        public decimal Remains { get; set; }

        public bool IsActive { get; set; }

        public bool IsSupplierActive { get; set; }

        private string _supplierName;
        public string SupplierName
        {
            get { return _supplierName; }
            set
            {
                _supplierName = value;
                OnPropertyChanged("SupplierName");
            }
        }

        private bool _isOfContractedSupplier;
        public bool IsOfContractedSupplier
        {
            get { return _isOfContractedSupplier; }
            set
            {
                _isOfContractedSupplier = value;
                OnPropertyChanged("IsOfContractedSupplier");
            }
        }

        private decimal _priceForClient;
        public decimal PriceForClient
        {
            get { return _priceForClient; }
            set
            {
                _priceForClient = value;
                OnPropertyChanged("PriceForClient");
            }
        }

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

        private string _quantityUnit;
        public string QuantityUnit
        {
            get { return _quantityUnit; }
            set
            {
                _quantityUnit = value;
                OnPropertyChanged("QuantityUnit");
            }
        }


        private decimal _orderQuantityBeforeUserChanges;
        public decimal OrderQuantityBeforeUserChanges
        {
            get { return _orderQuantityBeforeUserChanges; }
            set
            {
                _orderQuantityBeforeUserChanges = value;
                OnPropertyChanged("OrderQuantityBeforeUserChanges");
            }
        }

        public bool IsQuantityWasChanged
        {
            get { return OrderQuantity != OrderQuantityBeforeUserChanges; }
        }
    }
}
