using ClientApp_Mobile.ViewModels.SubPages;
using ClientApp_Mobile.Views;
using ClientApp_Mobile.Views.SubPages;
using Core.DBModels;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.Services
{
    public class ShellPageService
    {
        public static async void GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
        public static async void GoToSearchPage()
        {
            await Shell.Current.GoToAsync("Search");
        }

        public static async void GotoMidCategoriesPage(string selectedTopCategoryId, string selectedTopCategoryName)
        {
            await Shell.Current.GoToAsync($"MidCategories?categoryId={selectedTopCategoryId}&categoryName={selectedTopCategoryName}");
        }

        public static async void GotoProductCategoriesPage(string selectedMidCategoryId, string selectedMidCategoryName)
        {
            await Shell.Current.GoToAsync($"ProductCategories?categoryId={selectedMidCategoryId}&categoryName={selectedMidCategoryName}");
        }

        public static async void GotoOffersPage(string title, string categoryFilter = "", string supplierFilter = "", string searchText = "")
        {
            await Shell.Current.GoToAsync($"Offers?title={title}&catFilter={categoryFilter}&supFilter={supplierFilter}&search={searchText}");
            OffersSubPage pg = (OffersSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
            OffersSubPageVM bc = (OffersSubPageVM)pg.BindingContext;
            List<Guid> parsedCategoryFilter = string.IsNullOrEmpty(pg.CategoryFilterParam) ? null : pg.CategoryFilterParam.Split(',').Select(s => new Guid(s)).ToList();
            List<Guid> parsedSupplierFilter = string.IsNullOrEmpty(pg.SuppliersFilterParam) ? null : pg.SuppliersFilterParam.Split(',').Select(s => new Guid(s)).ToList();
            bc.Title = pg.TitleParam;
            bc.QueryDb(pg.ShowFavoritesOnly, parsedCategoryFilter, parsedSupplierFilter, pg.SearchTextParam);
        }

        public static async void GotoProductPage(Product product)
        {
            await Shell.Current.GoToAsync($"Product");
            ProductSubPage pg = (ProductSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
            ProductSubPageVM bc = new ProductSubPageVM(product);
            pg.BindingContext = bc;
            bc.QueryDb();
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
            await Shell.Current.GoToAsync("CurrentRequestConfirm");
            CurrentRequestConfirmSubPage pg = (CurrentRequestConfirmSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
            CurrentRequestConfirmSubPageVM bc = new CurrentRequestConfirmSubPageVM(requestsToAdd);
            pg.BindingContext = bc;
        }

        public static async void GotoArchivedRequestPage(ArchivedRequest request)
        {
            await Shell.Current.GoToAsync("ArchivedRequest");
            ArchivedRequestSubPage pg = (ArchivedRequestSubPage)(Shell.Current?.CurrentItem?.CurrentItem as IShellSectionController)?.PresentedPage;
            ArchivedRequestSubPageVM bc = new ArchivedRequestSubPageVM(request);
            pg.BindingContext = bc;
        }

        public ShellPageService()
        {

        }
    }
}
