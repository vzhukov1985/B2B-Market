using ClientApp_Mobile.Services;
using ClientApp_Mobile.Views.SubPages;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
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
    class OffersSubPageVM : BaseVM
    {
        private ObservableCollection<ProductWithOffersView> _products;
        public ObservableCollection<ProductWithOffersView> Products
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

        public async void QueryDb(List<Guid> categoryFilter = null, List<Guid> supplierFilter = null, string searchText = "", bool queryFavoritesOnly = false)
        {
            IsBusy = true;
            List<ProductWithOffersView> productsFromDb;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    db.Database.OpenConnection();
                    IQueryable<Product> queryPart;

                    if (queryFavoritesOnly)
                    {
                        Title = "Избранное";
                        queryPart = db.Favorites
                                      .Where(f => f.ClientUserId == AppSettings.CurrentUser.Id)
                                      .Select(f => f.Product);
                    }
                    else
                    {
                        queryPart = db.Products
                                      .Where(p => p.Offers.Any(of => of.Supplier.IsActive == true && of.Remains > 0 && of.IsActive == true))
                                      .Where(p => categoryFilter == null ? true : categoryFilter.Contains(p.CategoryId))
                                      .Where(p => supplierFilter == null ? true : p.Offers.Select(of => new { of.SupplierId, of.Remains }).Any(of => supplierFilter.Contains(of.SupplierId) && of.Remains > 0))
                                      .Where(p => string.IsNullOrEmpty(searchText) ? true : EF.Functions.Like(p.Name, $"%{searchText}%") || EF.Functions.Like(p.Category.Name, $"%{searchText}%"));
                    }

                    productsFromDb = await queryPart
                                       .OrderBy(p => p.Category.Name)
                                       .ThenBy(p => p.Name)
                                       .Select(p => new ProductWithOffersView
                                       {
                                           Id = p.Id,
                                           Name = p.Name,
                                           CategoryName = p.Category.Name,
                                           PictureUri = p.PictureUri,
                                           VolumeType = p.VolumeType.Name,
                                           Volume = p.Volume,
                                           VolumeUnit = p.VolumeUnit.ShortName,
                                           IsOfContractedSupplier = p.Offers.Any(o => ContractedSuppliersIds.Contains(o.Supplier.Id)),
                                           BestRetailOffer = db.Offers
                                                               .Where(o => o.ProductId == p.Id && o.IsActive == true && o.Supplier.IsActive == true)
                                                               .OrderByDescending(o => ContractedSuppliersIds.Contains(o.SupplierId))
                                                               .ThenBy(o => o.RetailPrice)
                                                               .Select(o => new BestOffer
                                                               {
                                                                   Price = o.RetailPrice,
                                                                   QuantityUnit = o.QuantityUnit.ShortName,
                                                                   SupplierName = o.Supplier.ShortName
                                                               }).FirstOrDefault(),
                                           BestDiscountOffer = db.Offers
                                                               .Where(o => o.ProductId == p.Id && o.IsActive == true && o.Supplier.IsActive == true)
                                                               .OrderByDescending(o => ContractedSuppliersIds.Contains(o.SupplierId))
                                                               .ThenBy(o => o.DiscountPrice)
                                                               .Select(o => new BestOffer
                                                               {
                                                                   Price = o.DiscountPrice,
                                                                   QuantityUnit = o.QuantityUnit.ShortName,
                                                                   SupplierName = o.Supplier.ShortName
                                                               }).FirstOrDefault(),
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

            foreach (var product in productsFromDb)
            {
                if (CTS.Token.IsCancellationRequested) { IsBusy = false; return; }
                product.IsFavoriteForUser = AppSettings.CurrentUser.Favorites.Select(f => f.ProductId).Contains(product.Id);
            }
            Device.BeginInvokeOnMainThread(() => Products = new ObservableCollection<ProductWithOffersView>(productsFromDb));
            IsBusy = false;
        }


        public void AddRemoveProductToFavorites(ProductWithOffersView product)
        {
            try
            {
                MarketDbContext.AddRemoveProductToFavourites(new Product { Id = product.Id, IsFavoriteForUser = product.IsFavoriteForUser }, AppSettings.CurrentUser);
                product.IsFavoriteForUser = !product.IsFavoriteForUser;
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                IsBusy = false;
                return;
            }
        }

        public Command AddRemoveProductToFavouritesCommand { get; }
        public Command ShowProductCommand { get; }

        public OffersSubPageVM()
        {
            ContractedSuppliersIds = AppSettings.CurrentUser.Client.ContractedSuppliersIDs;

            AddRemoveProductToFavouritesCommand = new Command<ProductWithOffersView>(p => Task.Run(() => AddRemoveProductToFavorites(p)));
            ShowProductCommand = new Command<ProductWithOffersView>(p => ShellPageService.GotoProductPage(new Product
            { 
                Id = p.Id,
                Name = p.Name,
                IsOfContractedSupplier = p.IsOfContractedSupplier,
                IsFavoriteForUser = p.IsFavoriteForUser,
                Category = new ProductCategory { Name = p.CategoryName},
                VolumeType = new VolumeType { Name = p.VolumeType},
                Volume = p.Volume,
                VolumeUnit = new VolumeUnit { ShortName = p.VolumeUnit}
            }));
        }
    }
    class ProductWithOffersView:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Uri PictureUri { get; set; }
        public bool IsOfContractedSupplier { get; set; }
        private bool _isFavoriteForUser;
        public bool IsFavoriteForUser
        {
            get { return _isFavoriteForUser; }
            set
            {
                _isFavoriteForUser = value;
                OnPropertyChanged("IsFavoriteForUser");
            }
        }
        public string CategoryName { get; set; }
        public string VolumeType { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; }
        public BestOffer BestRetailOffer { get; set; }
        public BestOffer BestDiscountOffer { get; set; }
    }

    class BestOffer
    {
        public decimal Price { get; set; }
        public string QuantityUnit { get; set; }
        public string SupplierName { get; set; }
    }
}
