using ClientApp.Services;
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

        private ObservableCollection<Guid> _contractedSuppliersIds;
        public ObservableCollection<Guid> ContractedSuppliersIds
        {
            get { return _contractedSuppliersIds; }
            set
            {
                _contractedSuppliersIds = value;
                OnPropertyChanged("ContractedSuppliersIds");
            }
        }

        private ObservableCollection<Guid> _favoriteProductsIds;
        public ObservableCollection<Guid> FavoriteProductsIds
        {
            get { return _favoriteProductsIds; }
            set
            {
                _favoriteProductsIds = value;
                OnPropertyChanged("FavoriteProductsIds");
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private async void QueryDb(bool ShowFavoritesOnly = false, List<Guid> categoryFilter = null, List<Guid> supplierFilter = null, string searchText = "")
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                ContractedSuppliersIds = new ObservableCollection<Guid>(User.Client.Contracts.Select(p => p.Supplier.Id).ToList());

                FavoriteProductsIds = new ObservableCollection<Guid>(User.FavoriteProducts.Select(f => f.Product.Id).ToList());
                
                Products = new ObservableCollection<Product>(await db.Products
                     .Include(p => p.Offers)
                     .ThenInclude(o => o.QuantityUnit)
                     .Include(p => p.Offers)
                     .ThenInclude(o => o.Supplier)
                     .Include(p => p.VolumeUnit)
                     .Include(p => p.VolumeType)
                     .Include(p => p.Category)
                     .Where(p => p.Offers.Any(of => of.Supplier.IsActive == true && of.Remains > 0 && of.IsActive == true && of.IsChecked == true))
                     .Where(p => categoryFilter == null ? true : categoryFilter.Contains(p.CategoryId))
                     .Where(p => supplierFilter == null ? true : p.Offers.Select(of => of.SupplierId).Any(id => supplierFilter.Contains(id)))
                     .Where(p => ShowFavoritesOnly ? FavoriteProductsIds.Contains(p.Id) : true)
                     .Where(p => searchText == "" ? true : (p.Name.Contains(searchText)) || (p.Category.Name.Contains(searchText)))
                     .OrderByDescending(p => p.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id)))
                     .ThenBy(p => p.Category.Name)
                     .ThenBy(p => p.Name)
                     .ToListAsync()
                     );
            }

            foreach (Product product in Products)
            {
                product.IsOfContractedSupplier = product.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id));
                product.IsFavoriteForUser = FavoriteProductsIds.Contains(product.Id);
                product.BestRetailPriceOffer = product.Offers.OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id)).ThenBy(o => o.RetailPrice).FirstOrDefault();
                product.BestDiscountPriceOffer = product.Offers.OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id)).ThenBy(o => o.DiscountPrice).FirstOrDefault();
            }
        }

        public CommandType ShowSearchSubPageCommand { get; }
        public CommandType AddRemoveProductToFavouritesCommand { get; }
        public CommandType ShowProductCommand { get; }
        public CommandType NavigationBackCommand { get; }

        public OffersSubPageVM(ClientUser user, IPageService pageService, string title, bool ShowFavoritesOnly = false, List<Guid> categoryFilter = null, List<Guid> supplierFilter = null, string searchText = "")
        {
            PageService = pageService;
            Title = title;
            User = user;

            ShowSearchSubPageCommand = new CommandType();
            ShowSearchSubPageCommand.Create(_ => PageService.ShowSearchSubPage(User));
            AddRemoveProductToFavouritesCommand = new CommandType();
            AddRemoveProductToFavouritesCommand.Create(p => MarketDbContext.AddRemoveProductToFavourites((Product)p, User));
            ShowProductCommand = new CommandType();
            ShowProductCommand.Create(p => PageService.ShowProductSubPage(User, (Product)p));
            NavigationBackCommand = new CommandType();
            NavigationBackCommand.Create(_ => PageService.SubPageNavigationBack());

            QueryDb(ShowFavoritesOnly, categoryFilter, supplierFilter, searchText);
      }
    }
}
