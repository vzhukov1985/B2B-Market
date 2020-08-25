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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    public class ProductSubPageVM : BaseVM
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

        private ObservableCollection<OfferWithOrder> _offersWithOrders;
        public ObservableCollection<OfferWithOrder> OffersWithOrders
        {
            get { return _offersWithOrders; }
            set
            {
                _offersWithOrders = value;
                OnPropertyChanged("OffersWithOrders");
            }
        }

        private bool _areChangesWereMade;
        public bool AreChangesWereMade
        {
            get { return _areChangesWereMade; }
            set
            {
                _areChangesWereMade = value;
                OnPropertyChanged("AreChangesWereMade");
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

        private ObservableCollection<CurrentOrder> CurrentRequestOrders;
        private List<Offer> Offers;



        private void ProcessChanges()
        {
            AreChangesWereMade = false;
            foreach (OfferWithOrder offer in OffersWithOrders)
            {
                if (offer.OrderQuantity > offer.Remains)
                    offer.OrderQuantity = offer.Remains;
                if (offer.OrderQuantity < 0 || offer.IsActive == false || offer.Supplier.IsActive == false)
                    offer.OrderQuantity = 0;

                if (offer.IsQuantityWasChanged)
                    AreChangesWereMade = true;

                TotalSum = OffersWithOrders.Sum(o => o.OrderQuantity * o.PriceForClient);
                UpdateCurrentRequestCommand.ChangeCanExecute();
            }
        }

        private void UpdateCurrentRequest()
        {
            IsBusy = true;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    foreach (OfferWithOrder offer in OffersWithOrders)
                    {
                        if (offer.IsQuantityWasChanged)
                        {
                            CurrentOrder currentRequestOrder = CurrentRequestOrders.Where(o => o.OfferId == offer.Id).FirstOrDefault();

                            if (currentRequestOrder == null)
                            {
                                CurrentOrder newRequestOrder = new CurrentOrder
                                {
                                    ClientId = User.ClientId,
                                    Client = User.Client,
                                    OfferId = offer.Id,
                                    Offer = Offers.Where(of => of.Id == offer.Id).FirstOrDefault(),
                                    Quantity = offer.OrderQuantity
                                };
                                db.CurrentOrders.Add(CurrentOrder.CloneForDB(newRequestOrder));
                                var sup = CurrentRequestOrders.Where(c => c.Offer.SupplierId == newRequestOrder.Offer.SupplierId).Select(c => c.Offer.Supplier).FirstOrDefault();
                                newRequestOrder.Offer.Supplier = sup == null ? offer.Supplier : sup;
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
                            offer.OrderQuantityBeforeUserChanges = offer.OrderQuantity;
                        }
                    }
                    db.SaveChanges();
                }

                AreChangesWereMade = false;
                IsBusy = false;
            }
            catch
            {
                ShellDialogService.ShowConnectionErrorDlg();
                IsBusy = false;
                return;
            }
        }

        private async void GoBack()
        {
            if (AreChangesWereMade)
            {
                if (await ShellDialogService.ShowOkCancelDialog("Внимание! Изменения, которые вы сделали, не сохранены и будут сброшены при переходе с этой страницы", "Внимание!") == false)
                    return;
            }
            ShellPageService.GoBack();
        }

        public async void QueryDb()
        {
            IsBusy = true;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    Product.ExtraProperties = new ObservableCollection<ProductExtraProperty>(await db.ProductExtraProperties.AsNoTracking().Where(pep => pep.ProductId == Product.Id).Include(pep => pep.PropertyType).ToListAsync());
                    Product.Description = await db.ProductDescriptions.FindAsync(Product.Id);
                }

                List<Guid> ContractedSuppliersIds = User.Client.Contracts.Select(p => p.Supplier.Id).ToList();

                CurrentRequestOrders = User.Client.CurrentOrders;

                Offers = Product.Offers
                    .Where(o => ((o.Supplier.IsActive == true) && (o.IsActive == true) && (o.Remains > 0)) || (CurrentRequestOrders.Where(oo => oo.OfferId == o.Id).Select(oo => oo.Quantity).FirstOrDefault() > 0))
                    .OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id))
                    .ThenBy(o => o.Supplier.ShortName).ToList();

                OffersWithOrders = new ObservableCollection<OfferWithOrder>(Offers.Select(of => new OfferWithOrder
                {
                    Id = of.Id,
                    SupplierProductCode = of.SupplierProductCode,
                    IsActive = of.IsActive,
                    Product = of.Product,
                    ProductId = of.ProductId,
                    Supplier = of.Supplier,
                    SupplierId = of.SupplierId,
                    DiscountPrice = of.DiscountPrice,
                    RetailPrice = of.RetailPrice,
                    QuantityUnit = of.QuantityUnit,
                    QuantityUnitId = of.QuantityUnitId,
                    Remains = (of.IsActive) && (of.Supplier.IsActive) ? of.Remains : 0,

                    IsOfContractedSupplier = ContractedSuppliersIds.Contains(of.Supplier.Id),
                    PriceForClient = ContractedSuppliersIds.Contains(of.Supplier.Id) ? of.DiscountPrice : of.RetailPrice,
                    OrderQuantityBeforeUserChanges = CurrentRequestOrders.Where(o => o.OfferId == of.Id).Select(o => o.Quantity).FirstOrDefault(),
                    OrderQuantity = CurrentRequestOrders.Where(o => o.OfferId == of.Id).Select(o => o.Quantity).FirstOrDefault()
                }));

                foreach (var order in OffersWithOrders)
                {
                    order.PropertyChanged += (s, a) => { DecrementOrderCommand.ChangeCanExecute(); IncrementOrderCommand.ChangeCanExecute(); ProcessChanges(); };
                }

                ExtraPropsCVHeight = Product.ExtraProperties.Count * 18; //FontSize+5
                OffersCVHeight = OffersWithOrders.Count * 60 + 1;

                AreChangesWereMade = false;
                ProcessChanges();
                IsBusy = false;
            }
            catch
            {
                ShellDialogService.ShowConnectionErrorDlg();
                IsBusy = false;
                return;
            }
        }

        private void AddRemoveProductToFavourites(Product product)
        {
            IsBusy = true;
            try
            {
                MarketDbContext.AddRemoveProductToFavourites((Product)product, User);
                IsBusy = false;
            }
            catch
            {
                ShellDialogService.ShowConnectionErrorDlg();
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

            AddRemoveProductToFavouritesCommand = new Command(p => AddRemoveProductToFavourites(product));
            IncrementOrderCommand = new Command(o => ((OfferWithOrder)o).OrderQuantity++, o => o == null ? false : ((OfferWithOrder)o).OrderQuantity < ((OfferWithOrder)o).Remains);
            DecrementOrderCommand = new Command(o => ((OfferWithOrder)o).OrderQuantity--, o => o == null ? false : ((OfferWithOrder)o).OrderQuantity > 0);
            ChangesInOrderAreMadeCommand = new Command(_ => ProcessChanges());
            ShowProductPictureCommand = new Command(_ => ShellPageService.GotoProductPicturePage(Product.Id.ToString(), Product.Name));
            UpdateCurrentRequestCommand = new Command(_ => UpdateCurrentRequest(), _ => AreChangesWereMade);
            GoBackCommand = new Command(_ => GoBack());
        }
    }
}
