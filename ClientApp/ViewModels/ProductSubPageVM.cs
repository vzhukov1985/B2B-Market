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


        private void ProcessChanges()
        {
            AreChangesWereMade = false;
            TotalSum = 0;
            foreach (OrderByUser order in Orders)
            {
                if (order.IsQuantityWasChanged)
                {
                    AreChangesWereMade = true;
                }
                TotalSum += order.PriceForClient * order.OrderQuantity;
            }
        }

        private void UpdateCurrentRequest()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                foreach (OrderByUser order in Orders)
                {
                    if (order.IsQuantityWasChanged)
                    {
                        CurrentOrder currentRequestOrder = CurrentRequestOrders.Where(o => o.OfferId == order.Id).FirstOrDefault();

                        if (currentRequestOrder == null)
                        {
                            currentRequestOrder = new CurrentOrder
                            {
                                ClientId = User.ClientId,
                                OfferId = order.Id,
                                Quantity = order.OrderQuantity
                            };
                            db.CurrentOrders.Add(currentRequestOrder);
                        }
                        else
                        {
                            if (order.OrderQuantity == 0)
                            {
                                db.CurrentOrders.Remove(currentRequestOrder);
                            }
                            else
                            {

                                currentRequestOrder.Quantity = order.OrderQuantity;
                                db.CurrentOrders.Update(currentRequestOrder);
                            }
                        }

                        db.SaveChanges();

                        //TODO: Check if it can be done on the client side
                        CurrentRequestOrders = User.Client.CurrentOrders = new ObservableCollection<CurrentOrder>(db.CurrentOrders
                            .Include(o => o.Offer)
                            .ThenInclude(o => o.QuantityUnit)
                            .Include(o => o.Offer)
                            .ThenInclude(o => o.Supplier));
                        
                        order.OrderQuantityBeforeUserChanges = order.OrderQuantity;
                    }
                }

            }
            AreChangesWereMade = false;
        }

        private ObservableCollection<CurrentOrder> CurrentRequestOrders;
        private readonly List<Offer> Offers;

        public CommandType ShowSearchSubPageCommand { get; }
        public CommandType NavigationBackCommand { get; }
        public CommandType AddRemoveProductToFavouritesCommand { get; }

        public CommandType IncrementOrderCommand { get; }
        public CommandType DecrementOrderCommand { get; }
        public CommandType ChangesAreMadeCommand { get; }

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
            IncrementOrderCommand.Create(o => ((OrderByUser)o).OrderQuantity++, o => o == null ? false : ((OrderByUser)o).OrderQuantity < ((OrderByUser)o).Remains);
            DecrementOrderCommand = new CommandType();
            DecrementOrderCommand.Create(o => ((OrderByUser)o).OrderQuantity--, o => o == null ? false : ((OrderByUser)o).OrderQuantity > 0);
            ChangesAreMadeCommand = new CommandType();
            ChangesAreMadeCommand.Create(_ => ProcessChanges());
            UpdateCurrentRequestCommand = new CommandType();
            UpdateCurrentRequestCommand.Create(_ => UpdateCurrentRequest(), _ => AreChangesWereMade);

            List<Guid> ContractedSuppliersIds = User.Client.Contracts.Select(p => p.Supplier.Id).ToList();

            Offers = Product.Offers
                .OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id))
                .ThenBy(o => o.Supplier.ShortName).ToList();

            CurrentRequestOrders = User.Client.CurrentOrders;

            Orders = new ObservableCollection<OrderByUser>();
            foreach (Offer offer in Offers)
            {
                OrderByUser orderToAdd = new OrderByUser
                {
                    Id = offer.Id,
                    Product = offer.Product,
                    ProductId = offer.ProductId,
                    Supplier = offer.Supplier,
                    SupplierId = offer.SupplierId,
                    DiscountPrice = offer.DiscountPrice,
                    RetailPrice = offer.RetailPrice,
                    QuantityUnit = offer.QuantityUnit,
                    QuantityUnitId = offer.QuantityUnitId,
                    Remains = offer.Remains,
                    IsOfContractedSupplier = ContractedSuppliersIds.Contains(offer.Supplier.Id)
                };

                if (orderToAdd.IsOfContractedSupplier)
                    orderToAdd.PriceForClient = offer.DiscountPrice;
                else
                    orderToAdd.PriceForClient = offer.RetailPrice;
                orderToAdd.OrderQuantity = orderToAdd.OrderQuantityBeforeUserChanges = CurrentRequestOrders.Where(o => o.OfferId == offer.Id).Select(o => o.Quantity).FirstOrDefault();

                Orders.Add(orderToAdd);
            }
            AreChangesWereMade = false;
            ProcessChanges();
        }
    }
}
