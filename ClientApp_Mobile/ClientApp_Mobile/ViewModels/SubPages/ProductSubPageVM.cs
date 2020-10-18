using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Models;
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

        private CurrentUserInfo _user;
        public CurrentUserInfo User
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

        private List<CurrentOrder> ProductCurrentOrders { get; set; }

        public Command AddRemoveProductToFavouritesCommand { get; }
        public Command IncrementOrderCommand { get; }
        public Command DecrementOrderCommand { get; }
        public Command ChangesInOrderAreMadeCommand { get; }
        public Command ShowProductPictureCommand { get; }

        public Command UpdateCurrentRequestCommand { get; }
        public Command GoBackCommand { get; }

        public ProductSubPageVM(Product product)
        {
            Product = product;
            User = AppSettings.CurrentUser;
            AddRemoveProductToFavouritesCommand = new Command(_ => Task.Run(() => AddRemoveProductToFavorites()));
            IncrementOrderCommand = new Command<OfferWithOrderView>(o => o.OrderQuantity++, o => o == null ? false : o.OrderQuantity < o.Remains);
            DecrementOrderCommand = new Command<OfferWithOrderView>(o => o.OrderQuantity--, o => o == null ? false : o.OrderQuantity > 0);
            ChangesInOrderAreMadeCommand = new Command(_ => ProcessChanges());
            ShowProductPictureCommand = new Command(_ => ShellPageService.GotoProductPicturePage(Product.Id.ToString(), Product.Name));
            UpdateCurrentRequestCommand = new Command(_ => Task.Run(() => UpdateCurrentRequest()), _ => AreChangesWereMade);
            GoBackCommand = new Command(_ => GoBack());
        }

        public async void QueryDb()
        {
            IsBusy = true;
            List<Offer> offersFromDb;

            Product.ExtraProperties = new ObservableCollection<ProductExtraProperty>(await ApiConnect.GetExtraPropertiesByProduct(Product.Id));
            Product.Description = new ProductDescription { Text = await ApiConnect.GetProductDescription(Product.Id) };
            offersFromDb = await ApiConnect.GetOffersByProduct(Product.Id);

            List<Guid> ContractedSupplierIds = AppSettings.CurrentUser.Client.ContractedSuppliersIDs;

            ProductCurrentOrders = await ApiConnect.GetCurrentOrdersByProduct(AppSettings.CurrentUser.Client.Id, Product.Id);

            OffersWithOrders = offersFromDb
                .Where(odb => ((odb.Supplier.IsActive == true) && (odb.IsActive == true) && (odb.Remains > 0)) || (ProductCurrentOrders.Where(oo => oo.OfferId == odb.Id).Select(oo => oo.Quantity).FirstOrDefault() > 0))
                .Select(odb => new OfferWithOrderView
                {
                    Id = odb.Id,
                    IsActive = odb.IsActive,
                    IsOfContractedSupplier = ContractedSupplierIds.Contains(odb.Supplier.Id),
                    IsSupplierActive = odb.Supplier.IsActive,
                    SupplierName = odb.Supplier.ShortName,
                    OrderQuantity = ProductCurrentOrders.Where(o => o.OfferId == odb.Id).Select(o => o.Quantity).FirstOrDefault(),
                    QuantityUnit = odb.QuantityUnit.ShortName,
                    OrderQuantityBeforeUserChanges = ProductCurrentOrders.Where(o => o.OfferId == odb.Id).Select(o => o.Quantity).FirstOrDefault(),
                    PriceForClient = ContractedSupplierIds.Contains(odb.SupplierId) ? odb.DiscountPrice : odb.RetailPrice,
                    Remains = odb.Remains
                }).OrderByDescending(oo => oo.IsOfContractedSupplier).ThenBy(oo => oo.SupplierName).ToList();

            foreach (var order in OffersWithOrders)
            {
                order.PropertyChanged += (s, a) =>
                {
                    if (a.PropertyName == "OrderQuantity") ProcessChanges();
                };
            }

            ProcessChanges();

            ExtraPropsCVHeight = Product.ExtraProperties.Count * 18; //FontSize+5
            OffersCVHeight = OffersWithOrders.Count * 60 + 1;
            AreChangesWereMade = false;
            IsBusy = false;
        }

        private void UpdateCurrentRequest()
        {
            IsBusy = true;

            foreach (var offer in OffersWithOrders)
            {
                if (offer.IsQuantityWasChanged)
                {
                    CurrentOrder updatedOrder = ApiConnect.UpdateCurrentOrder(new CurrentOrder { ClientId = User.Client.Id, OfferId = offer.Id, Quantity = offer.OrderQuantity }).Result;

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        offer.OrderQuantity = updatedOrder.Quantity;
                        offer.OrderQuantityBeforeUserChanges = offer.OrderQuantity;
                    });
                }
            }

            Device.BeginInvokeOnMainThread(() => ProcessChanges());
            AreChangesWereMade = false;
            IsBusy = false;
        }

        private void ProcessChanges()
        {
            Device.BeginInvokeOnMainThread(() =>
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
            });
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



        public void AddRemoveProductToFavorites()
        {
            var user = AppSettings.CurrentUser;
            ApiConnect.AddRemoveProductToFavorites(Product.Id.ToString());
            if (Product.IsFavoriteForUser)
            {
                user.FavoriteProductsIds.Remove(Product.Id);
                Product.IsFavoriteForUser = false;
            }
            else
            {
                user.FavoriteProductsIds.Add(Product.Id);
                Product.IsFavoriteForUser = true;
            }
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
