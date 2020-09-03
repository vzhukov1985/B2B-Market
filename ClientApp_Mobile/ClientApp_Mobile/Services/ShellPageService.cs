using ClientApp_Mobile.ViewModels;
using ClientApp_Mobile.ViewModels.SubPages;
using ClientApp_Mobile.Views;
using ClientApp_Mobile.Views.SubPages;
using Core.DBModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.Services
{
    public class ShellPageService
    {
        private static object locker = new object();
        private static bool isGoing = false;

        public static async void GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
        public static async void GoToSearchPage()
        {
            await Shell.Current.GoToAsync("Search");
        }

        public static async void GotoMidCategoriesPage(TopCategory selectedTopCategory)
        {
            lock (locker)
            {
                if (isGoing)
                    return;
                else
                    isGoing = true;
            }
            if (selectedTopCategory != null)
            {
                await Shell.Current.GoToAsync($"MidCategories");
                CategoriesSubPage pg = (CategoriesSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
                MidCategoriesSubPageVM bc = new MidCategoriesSubPageVM(selectedTopCategory);
                pg.BindingContext = bc;
            }
            isGoing = false;
        }

        public static async void GotoProductCategoriesPage(MidCategory selectedMidCategory, List<ProductCategory> subCategories)
        {
            lock (locker)
            {
                if (isGoing)
                    return;
                else
                    isGoing = true;
            }
            if (selectedMidCategory != null)
            {
                await Shell.Current.GoToAsync($"ProductCategories");
                CategoriesSubPage pg = (CategoriesSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
                ProductCategoriesSubPageVM bc = new ProductCategoriesSubPageVM(selectedMidCategory, subCategories);
                pg.BindingContext = bc;
            }
            isGoing = false;
        }

        public static async void GotoOffersPage(string title, List<Guid> categoryFilter = null, List<Guid> supplierFilter = null, string searchText = "")
        {
            lock (locker)
            {
                if (isGoing)
                    return;
                else
                    isGoing = true;
            }
            await Shell.Current.GoToAsync($"Offers");
            OffersSubPage pg = (OffersSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
            OffersSubPageVM bc = (OffersSubPageVM)pg.BindingContext;
            bc.Title = title;
            await Task.Run(() => bc.QueryDb(categoryFilter, supplierFilter, searchText));
            isGoing = false;
        }

        public static async void GotoProductPage(Product product)
        {
            lock (locker)
            {
                if (isGoing)
                    return;
                else
                    isGoing = true;
            }
            if (product != null)
            {
                await Shell.Current.GoToAsync($"Product");
                ProductSubPage pg = (ProductSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
                ProductSubPageVM bc = new ProductSubPageVM(product);
                pg.BindingContext = bc;
                await Task.Run(() => bc.QueryDb());
            }
            isGoing = false;
        }

        public static async void GotoProductPicturePage(string productId, string productName)
        {
            await Shell.Current.GoToAsync($"ProductPicture?id={productId}&name={productName}");
        }

        public static async void GotoCurrentRequestPage()
        {
            await Shell.Current.GoToAsync("//Main/TabCurrentRequest");
        }

        public static async void GotoCurrentRequestConfirmPage(List<ArchivedRequest> requestsToAdd)
        {
            lock (locker)
            {
                if (isGoing)
                    return;
                else
                    isGoing = true;
            }
            if (requestsToAdd != null)
            {
                await Shell.Current.GoToAsync("CurrentRequestConfirm");
                CurrentRequestConfirmSubPage pg = (CurrentRequestConfirmSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
                CurrentRequestConfirmSubPageVM bc = new CurrentRequestConfirmSubPageVM(requestsToAdd);
                pg.BindingContext = bc;
            }
            isGoing = false;
        }

        public static async void GotoArchivedRequestPage(ArchivedRequest request)
        {
            lock (locker)
            {
                if (isGoing)
                    return;
                else
                    isGoing = true;
            }
            if (request != null)
            {
                await Shell.Current.GoToAsync("ArchivedRequest");
                ArchivedRequestSubPage pg = (ArchivedRequestSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
                ArchivedRequestSubPageVM bc = new ArchivedRequestSubPageVM(request);
                pg.BindingContext = bc;
            }
            isGoing = false;
        }

        public ShellPageService()
        {

        }
    }
}
