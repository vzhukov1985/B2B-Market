using ClientApp_Mobile.Services;
using ClientApp_Mobile.Views.SubPages;
using Core.DBModels;
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
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    public class OffersSubPageVM : BaseVM
    {
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

        public async void QueryDb(bool ShowFavoritesOnly = false, List<Guid> categoryFilter = null, List<Guid> supplierFilter = null, string searchText = "")
        {
            IsBusy = true;
            try
            {
                ContractedSuppliersIds = new ObservableCollection<Guid>(UserService.CurrentUser.Client.Contracts.Select(p => p.Supplier.Id).ToList());
                FavoriteProductsIds = new ObservableCollection<Guid>(UserService.CurrentUser.FavoriteProducts.Select(f => f.Product.Id).ToList());

                List<Product> unsortedProductsList;
                using (MarketDbContext db = new MarketDbContext())
                {
                    unsortedProductsList = await db.Products
                         .Include(p => p.Offers)
                         .ThenInclude(o => o.QuantityUnit)
                         .Include(p => p.Offers)
                         .ThenInclude(o => o.Supplier)
                         .Include(p => p.VolumeUnit)
                         .Include(p => p.VolumeType)
                         .Include(p => p.Category)
                         .Where(p => p.Offers.Any(of => of.Supplier.IsActive == true && of.Remains > 0 && of.IsActive == true))
                         .Where(p => categoryFilter == null ? true : categoryFilter.Contains(p.CategoryId))
                         .Where(p => supplierFilter == null ? true : p.Offers.Select(of => of.SupplierId).Any(id => supplierFilter.Contains(id)))
                         .Where(p => ShowFavoritesOnly ? FavoriteProductsIds.Contains(p.Id) : true)
                         .Where(p => string.IsNullOrEmpty(searchText) ? true : EF.Functions.Like(p.Name, $"%{searchText}%") || EF.Functions.Like(p.Category.Name, $"%{searchText}%"))
                         .ToListAsync();
                }

                foreach (Product product in unsortedProductsList)
                {
                    product.IsOfContractedSupplier = product.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id));
                    product.IsFavoriteForUser = FavoriteProductsIds.Contains(product.Id);
                    product.BestRetailPriceOffer = product.Offers.OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id)).ThenBy(o => o.RetailPrice).FirstOrDefault();
                    product.BestDiscountPriceOffer = product.Offers.OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id)).ThenBy(o => o.DiscountPrice).FirstOrDefault();
                }

                Products = new ObservableCollection<Product>(unsortedProductsList.OrderBy(p => p.Category.Name).ThenBy(p => p.Name));
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
                MarketDbContext.AddRemoveProductToFavourites((Product)product, UserService.CurrentUser);
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
        public Command ShowProductCommand { get; }

        public OffersSubPageVM()
        {
            AddRemoveProductToFavouritesCommand = new Command(p => AddRemoveProductToFavourites((Product)p));
            ShowProductCommand = new Command<Product>(p => ShellPageService.GotoProductPage(p));
        }
    }
}
