using ClientApp_Mobile.Services;
using ClientApp_Mobile.Views.SubPages;
using Core.DBModels;
using Core.Models;
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
        private ObservableCollection<ProductWithOffersDbView> _products;
        public ObservableCollection<ProductWithOffersDbView> Products
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

        public Command AddRemoveProductToFavouritesCommand { get; }
        public Command ShowProductCommand { get; }

        public OffersSubPageVM()
        {
            ContractedSuppliersIds = AppSettings.CurrentUser.Client.ContractedSuppliersIDs;

            AddRemoveProductToFavouritesCommand = new Command<ProductWithOffersDbView>(p => Task.Run(() => AddRemoveProductToFavorites(p)));
            ShowProductCommand = new Command<ProductWithOffersDbView>(p => ShellPageService.GotoProductPage(new Product
            {
                Id = p.Id,
                Name = p.Name,
                IsOfContractedSupplier = p.IsOfContractedSupplier,
                IsFavoriteForUser = p.IsFavoriteForUser,
                Category = new ProductCategory { Name = p.CategoryName },
                VolumeType = new VolumeType { Name = p.VolumeType },
                Volume = p.Volume,
                VolumeUnit = new VolumeUnit { ShortName = p.VolumeUnit }
            }));
        }

        public async void QueryDb(List<Guid> categoryFilter = null, List<Guid> supplierFilter = null, string searchText = "", bool queryFavoritesOnly = false)
        {
            IsBusy = true;
            if (queryFavoritesOnly)
                Title = "Избранное";

            List<ProductWithOffersDbView> productsFromDb = await ApiConnect.GetOffers(categoryFilter, supplierFilter, searchText, queryFavoritesOnly, ContractedSuppliersIds, CTS.Token);

            foreach (var product in productsFromDb)
            {
                if (CTS.Token.IsCancellationRequested) { IsBusy = false; return; }
                product.IsFavoriteForUser = AppSettings.CurrentUser.FavoriteProductsIds.Contains(product.Id);
            }
            Device.BeginInvokeOnMainThread(() => Products = new ObservableCollection<ProductWithOffersDbView>(productsFromDb));
            IsBusy = false;
        }


        public void AddRemoveProductToFavorites(ProductWithOffersDbView product)
        {
            var user = AppSettings.CurrentUser;
            ApiConnect.AddRemoveProductToFavorites(product.Id.ToString());
            if (product.IsFavoriteForUser)
            {
                user.FavoriteProductsIds.Remove(product.Id);
                product.IsFavoriteForUser = false;
            }
            else
            {
                user.FavoriteProductsIds.Add(product.Id);
                product.IsFavoriteForUser = true;
            }
        }
    }
}
