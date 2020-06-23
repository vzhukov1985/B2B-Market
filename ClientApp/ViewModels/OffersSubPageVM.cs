using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
    public class OffersSubPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
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

        private ObservableCollection<Supplier> _contractedSuppliers;
        public ObservableCollection<Supplier> ContractedSuppliers
        {
            get { return _contractedSuppliers; }
            set
            {
                _contractedSuppliers = value;
                OnPropertyChanged("ContractedSuppliers");
            }
        }

        private ObservableCollection<Product> _favoriteProducts;
        public ObservableCollection<Product> FavoriteProducts
        {
            get { return _favoriteProducts; }
            set
            {
                _favoriteProducts = value;
                OnPropertyChanged("FavoriteProducts");
            }
        }

        private void AddRemoveProductToFavourites(Product selectedProduct)
        {
            using (MarketDbContext db = new MarketDbContext())
            {

                if (selectedProduct.IsFavoriteForUser)
                {
                    selectedProduct.IsFavoriteForUser = false;
                    Favorite favoriteToRemove=db.Favorites.Where(f => (f.ClientUserId == User.Id) && (f.ProductId == selectedProduct.Id)).FirstOrDefault();
                    db.Favorites.Remove(favoriteToRemove);
                }
                else
                {
                    selectedProduct.IsFavoriteForUser = true;
                    db.Favorites.Add(new Favorite
                    {
                        ClientUserId = User.Id,
                        ProductId = selectedProduct.Id
                    });
                }
                db.SaveChanges();
            }
        }

        public CommandType AddRemoveProductToFavouritesCommand { get; }

        public OffersSubPageVM(ClientUser user)
        {
            User = user;
            AddRemoveProductToFavouritesCommand = new CommandType();
            AddRemoveProductToFavouritesCommand.Create(p => { AddRemoveProductToFavourites((Product)p); });

            using (MarketDbContext db = new MarketDbContext())
            {
                ContractedSuppliers = new ObservableCollection<Supplier>(db.Suppliers
                    .Include(cc => cc.Contracts)
                    .Where(dd => dd.Contracts.Any(ee => ee.ClientId == User.ClientId))
                    .ToList());

                FavoriteProducts = new ObservableCollection<Product>(db.Products
                    .Include(p => p.Favorites)
                    .Where(p => p.Favorites.Any(f => f.ClientUserId == User.Id))
                    .ToList());

                 Products = new ObservableCollection<Product>(db.Products
                     .Include(p => p.Offers)
                     .ThenInclude(o => o.QuantityUnit)
                     .Include(p => p.Offers)
                     .ThenInclude(o => o.Supplier)
                     .Include(p => p.VolumeUnit)
                     .Include(p => p.VolumeType)
                     .Include(p => p.Category)
                     .OrderByDescending(p => p.Offers.Any(o => ContractedSuppliers.Contains(o.Supplier)))
                     .ThenBy(p => p.Category.Name)
                     .ThenBy(p => p.Name)
                     .ToList()
                     );

                foreach (Product product in Products)
                {
                    product.IsOfContractedSupplier = product.Offers.Any(o => ContractedSuppliers.Contains(o.Supplier));
                    product.IsFavoriteForUser = FavoriteProducts.Contains(product);
                    product.BestRetailPriceOffer = product.Offers.OrderByDescending(o => ContractedSuppliers.Contains(o.Supplier)).ThenByDescending(o => o.RetailPrice).FirstOrDefault();
                    product.BestDiscountPriceOffer = product.Offers.OrderByDescending(o => ContractedSuppliers.Contains(o.Supplier)).ThenByDescending(o => o.DiscountPrice).FirstOrDefault();
                }
            }
        }
    }
}
