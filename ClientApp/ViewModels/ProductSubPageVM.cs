using ClientApp.Services;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClientApp.ViewModels
{
    public class OfferWithOrder : Offer
    {
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

        private int _orderQuantity;
        public int OrderQuantity
        {
            get { return _orderQuantity; }
            set
            {
                _orderQuantity = value;
                OnPropertyChanged("OrderQuantity");
            }
        }

        private int _orderQuantityBeforeUserChanges;
        public int OrderQuantityBeforeUserChanges
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


    public class ProductSubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
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

        private ObservableCollection<OfferWithOrder> _orders;
        public ObservableCollection<OfferWithOrder> OffersWithOrders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged("Orders");
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

        public decimal TotalSum
        {
            get { return OffersWithOrders.Sum(o => o.OrderQuantity * o.PriceForClient); }
        }


        private void ProcessChanges()
        {
            AreChangesWereMade = false;
            foreach (OfferWithOrder offer in OffersWithOrders)
            {
                if (offer.OrderQuantity > offer.Remains)
                    offer.OrderQuantity = offer.Remains;
                if (offer.OrderQuantity < 0 || offer.IsActive == false || offer.Supplier.IsActive == false || offer.IsChecked == false)
                    offer.OrderQuantity = 0;

                if (offer.IsQuantityWasChanged)
                    AreChangesWereMade = true;

                OnPropertyChanged("TotalSum");
            }
        }

        private void UpdateCurrentRequest()
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
                                OfferId = offer.Id,
                                Quantity = offer.OrderQuantity
                            };
                            db.CurrentOrders.Add(newRequestOrder);
                        }
                        else
                        {
                            if (offer.OrderQuantity == 0)
                            {
                                db.CurrentOrders.Remove(currentRequestOrder);
                            }
                            else
                            {

                                currentRequestOrder.Quantity = offer.OrderQuantity;
                                db.CurrentOrders.Update(currentRequestOrder);
                            }
                        }
                        offer.OrderQuantityBeforeUserChanges = offer.OrderQuantity;
                    }
                }
                db.SaveChanges();

                //TODO: Check if it can be done on the client side
                CurrentRequestOrders = User.Client.CurrentOrders = new ObservableCollection<CurrentOrder>(db.CurrentOrders
                    .Where(o => o.ClientId == User.ClientId)
                    .Include(o => o.Offer)
                    .ThenInclude(o => o.QuantityUnit)
                    .Include(o => o.Offer)
                    .ThenInclude(o => o.Supplier));
            }

            AreChangesWereMade = false;
        }

        private async void QueryDB()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                Product.ExtraProperties = new ObservableCollection<ProductExtraProperty>(await db.ProductExtraProperties.Where(pep => pep.ProductId == Product.Id).Include(pep => pep.PropertyType).ToListAsync());
                Product.Description = await db.ProductDescriptions.FindAsync(Product.Id);
            }
        }

        private ObservableCollection<CurrentOrder> CurrentRequestOrders;
        private readonly List<Offer> Offers;

        public CommandType ShowSearchSubPageCommand { get; }
        public CommandType NavigationBackCommand { get; }
        public CommandType AddRemoveProductToFavouritesCommand { get; }

        public CommandType IncrementOrderCommand { get; }
        public CommandType DecrementOrderCommand { get; }
        public CommandType ChangesInOrderAreMadeCommand { get; }

        public CommandType UpdateCurrentRequestCommand { get; }

        public ProductSubPageVM(ClientUser user, Product product, IPageService pageService)
        {
            User = user;
            Product = product;
            PageService = pageService;

            ShowSearchSubPageCommand = new CommandType();
            ShowSearchSubPageCommand.Create(_ => PageService.ShowSearchSubPage(User));
            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());
            AddRemoveProductToFavouritesCommand = new CommandType();
            AddRemoveProductToFavouritesCommand.Create(_ => MarketDbContext.AddRemoveProductToFavourites(Product, User));
            IncrementOrderCommand = new CommandType();
            IncrementOrderCommand.Create(o => ((OfferWithOrder)o).OrderQuantity++, o => o == null ? false : ((OfferWithOrder)o).OrderQuantity < ((OfferWithOrder)o).Remains);
            DecrementOrderCommand = new CommandType();
            DecrementOrderCommand.Create(o => ((OfferWithOrder)o).OrderQuantity--, o => o == null ? false : ((OfferWithOrder)o).OrderQuantity > 0);
            ChangesInOrderAreMadeCommand = new CommandType();
            ChangesInOrderAreMadeCommand.Create(_ => ProcessChanges());
            UpdateCurrentRequestCommand = new CommandType();
            UpdateCurrentRequestCommand.Create(_ => UpdateCurrentRequest(), _ => AreChangesWereMade);

            QueryDB();

            List<Guid> ContractedSuppliersIds = User.Client.Contracts.Select(p => p.Supplier.Id).ToList();

            CurrentRequestOrders = User.Client.CurrentOrders;

            Offers = Product.Offers
                .Where(o => ((o.Supplier.IsActive == true) && (o.IsActive == true) && (o.Remains > 0) && (o.IsChecked == true)) || (CurrentRequestOrders.Where(oo => oo.OfferId == o.Id).Select(oo => oo.Quantity).FirstOrDefault() > 0))
                .OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id))
                .ThenBy(o => o.Supplier.ShortName).ToList();

            OffersWithOrders = new ObservableCollection<OfferWithOrder>(Offers.Select(offer => new OfferWithOrder
            {
                Id = offer.Id,
                SupplierProductCode = offer.SupplierProductCode,
                IsActive = offer.IsActive,
                IsChecked = offer.IsChecked,
                Product = offer.Product,
                ProductId = offer.ProductId,
                Supplier = offer.Supplier,
                SupplierId = offer.SupplierId,
                DiscountPrice = offer.DiscountPrice,
                RetailPrice = offer.RetailPrice,
                QuantityUnit = offer.QuantityUnit,
                QuantityUnitId = offer.QuantityUnitId,
                Remains = (offer.IsActive) && (offer.Supplier.IsActive) && (offer.IsChecked) ? offer.Remains : 0,

                IsOfContractedSupplier = ContractedSuppliersIds.Contains(offer.Supplier.Id),
                PriceForClient = ContractedSuppliersIds.Contains(offer.Supplier.Id) ? offer.DiscountPrice : offer.RetailPrice,
                OrderQuantityBeforeUserChanges = CurrentRequestOrders.Where(o => o.OfferId == offer.Id).Select(o => o.Quantity).FirstOrDefault(),
                OrderQuantity = CurrentRequestOrders.Where(o => o.OfferId == offer.Id).Select(o => o.Quantity).FirstOrDefault()
            }));

            AreChangesWereMade = false;
            ProcessChanges();
        }
    }
}
