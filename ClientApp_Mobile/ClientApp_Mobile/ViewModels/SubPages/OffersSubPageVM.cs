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
using System.Threading;
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

        private List<Guid> _contractedSuppliersIds;
        public List<Guid> ContractedSuppliersIds
        {
            get { return _contractedSuppliersIds; }
            set
            {
                _contractedSuppliersIds = value;
                OnPropertyChanged("ContractedSuppliersIds");
            }
        }

        private List<Guid> _favoriteProductsIds;
        public List<Guid> FavoriteProductsIds
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0075:Simplify conditional expression", Justification = "<Pending>")]
        public async void QueryDb(List<Guid> categoryFilter = null, List<Guid> supplierFilter = null, string searchText = "")
        {
            IsBusy = true;
            try
            {
                if (CTS.Token.IsCancellationRequested) { IsBusy = false; return; }
                FavoriteProductsIds = UserService.CurrentUser.Favorites.Select(f => f.Product.Id).ToList();

                List<Product> unsortedProductsList;

                using (MarketDbContext db = new MarketDbContext())
                {
                    if (CTS.Token.IsCancellationRequested) { IsBusy = false; return; }

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
                                                   .Where(p => supplierFilter == null ? true : p.Offers.Select(of => new { of.SupplierId, of.Remains }).Any( of => supplierFilter.Contains(of.SupplierId) && of.Remains > 0))
                                                   .Where(p => string.IsNullOrEmpty(searchText) ? true : EF.Functions.Like(p.Name, $"%{searchText}%") || EF.Functions.Like(p.Category.Name, $"%{searchText}%"))
                                                   .ToListAsync(CTS.Token);
                }

                foreach (Product product in unsortedProductsList)
                {
                    if (CTS.Token.IsCancellationRequested) { IsBusy = false; return; }
                    product.IsOfContractedSupplier = product.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id));
                    product.IsFavoriteForUser = FavoriteProductsIds.Contains(product.Id);
                    product.BestRetailPriceOffer = product.Offers.OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id)).ThenBy(o => o.RetailPrice).FirstOrDefault();
                    product.BestDiscountPriceOffer = product.Offers.OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id)).ThenBy(o => o.DiscountPrice).FirstOrDefault();
                }

                if (CTS.Token.IsCancellationRequested) { IsBusy = false; return; }
                var sortedList = unsortedProductsList.OrderBy(p => p.Category.Name).ThenBy(p => p.Name).ToList();
                Device.BeginInvokeOnMainThread(() => Products = new ObservableCollection<Product>(sortedList));
                IsBusy = false;
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
        }

        public async Task<bool> QueryFavoritesOnly(bool queryFavoritesFromDb)
        {
            IsBusy = true;
            if (CTS.Token.IsCancellationRequested) { IsBusy = false; return false; }
            

            List<Favorite> unsortedFavoritesList;

            if (queryFavoritesFromDb)
            {
                Title = "Избранное";
                if (CTS.Token.IsCancellationRequested) { IsBusy = false; return false; }
                try
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        if (CTS.Token.IsCancellationRequested) { IsBusy = false; return false; }
                        UserService.CurrentUser.Favorites = await db.Favorites.Where(f => f.ClientUserId == UserService.CurrentUser.Id)
                                                                     .Include(f => f.Product)
                                                                     .ThenInclude(p => p.Offers)
                                                                     .ThenInclude(o => o.QuantityUnit)
                                                                     .Include(f => f.Product)
                                                                     .ThenInclude(p => p.Offers)
                                                                     .ThenInclude(o => o.Supplier)
                                                                     .Include(f => f.Product)
                                                                     .ThenInclude(p => p.VolumeUnit)
                                                                     .Include(f => f.Product)
                                                                     .ThenInclude(p => p.VolumeType)
                                                                     .Include(f => f.Product)
                                                                     .ThenInclude(p => p.Category)
                                                                     .ToListAsync(CTS.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    IsBusy = false;
                    return false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                    IsBusy = false;
                    return false;
                }
            }
            unsortedFavoritesList = UserService.CurrentUser.Favorites;

            foreach (Favorite favorite in unsortedFavoritesList)
            {
                if (CTS.Token.IsCancellationRequested) { IsBusy = false; return false; }
                favorite.Product.IsOfContractedSupplier = favorite.Product.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id));
                favorite.Product.IsFavoriteForUser = true;
                favorite.Product.BestRetailPriceOffer = favorite.Product.Offers.OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id)).ThenBy(o => o.RetailPrice).FirstOrDefault();
                favorite.Product.BestDiscountPriceOffer = favorite.Product.Offers.OrderByDescending(o => ContractedSuppliersIds.Contains(o.Supplier.Id)).ThenBy(o => o.DiscountPrice).FirstOrDefault();
            }

            if (CTS.Token.IsCancellationRequested) { IsBusy = false; return false; }
            var sortedFavoritesList = unsortedFavoritesList.Select(f => f.Product).OrderBy(p => p.Category.Name).ThenBy(p => p.Name).ToList();
            Device.BeginInvokeOnMainThread(() => Products = new ObservableCollection<Product>(sortedFavoritesList));
            IsBusy = false;
            return true;
        }

        public Command AddRemoveProductToFavouritesCommand { get; }
        public Command ShowProductCommand { get; }

        public OffersSubPageVM()
        {
            ContractedSuppliersIds = UserService.CurrentUser.Client.ContractedSuppliersIDs;

            AddRemoveProductToFavouritesCommand = new Command(p => Task.Run(() =>MarketDbContext.AddRemoveProductToFavourites((Product)p, UserService.CurrentUser)));
            ShowProductCommand = new Command<Product>(p => ShellPageService.GotoProductPage(p));
        }
    }
}
