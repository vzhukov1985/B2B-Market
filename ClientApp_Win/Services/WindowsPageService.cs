using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using ClientApp.Services;
using ClientApp_Win.Views;
using ClientApp.ViewModels;
using System.Security.RightsManagement;
using System.Windows.Automation.Peers;

namespace ClientApp_Win.Services
{
    public class WindowsPageService: IPageService
    {
        public static NavigationService NavigationService;
        public static NavigationService SubNavigationService;

        public void ShowAuthorizationPage()
        {
            NavigationService.Navigate(new AuthorizationPage());
        }

        public void ShowFirstLoginPage(ClientUser user)
        {
            FirstLoginPage firstLoginPage = new FirstLoginPage { DataContext = new FirstLoginPageVM<RelayCommand>(user, new WindowsDialogService(), new WindowsPageService()) };
            NavigationService.Navigate(firstLoginPage);
        }

        public void ShowMainPage(ClientUser user)
        {
            MainPage mainPage = new MainPage { DataContext = new MainPageVM<RelayCommand>(user, new WindowsPageService()) };
            NavigationService.Navigate(mainPage);
        }

        public void SubPageNavigationBack()
        {
            SubNavigationService.GoBack();
        }

        public void ShowMainSubPage(ClientUser user)
        {
            MainSubPage mainSubPage = new MainSubPage { DataContext = new MainSubPageVM<RelayCommand>(user, new WindowsPageService()) };
            SubNavigationService.Navigate(mainSubPage);
        }

        public void ShowMidCategoriesSubPage(ClientUser user, TopCategory topCategory)
        {
            CategoriesSubPage midCategoriesSubPage = new CategoriesSubPage();
            MidCategorySubPageVM<RelayCommand> dc = new MidCategorySubPageVM<RelayCommand>(user, topCategory, new WindowsPageService());
            midCategoriesSubPage.DataContext = dc;
            if (!dc.IsRedirectOnLoad)
                SubNavigationService.Navigate(midCategoriesSubPage);
        }

        public void ShowProductCategoriesSubPage(ClientUser user, MidCategory midCategory)
        {
            CategoriesSubPage midCategoriesSubPage = new CategoriesSubPage();
            ProductCategorySubPageVM<RelayCommand> dc = new ProductCategorySubPageVM<RelayCommand>(user, midCategory, new WindowsPageService());
            midCategoriesSubPage.DataContext = dc;
            if (!dc.IsRedirectOnLoad)
                SubNavigationService.Navigate(midCategoriesSubPage);
        }

        public void ShowOffersSubPage(ClientUser user, string title, List<Guid> categoryFilter, List<Guid> supplierFilter, string searchText = "")
        {
            OffersSubPage offersSubPage = new OffersSubPage { DataContext = new OffersSubPageVM<RelayCommand>(user, new WindowsPageService(), title, false, categoryFilter, supplierFilter, searchText) };
            SubNavigationService.Navigate(offersSubPage);
        }
        public void ShowProductSubPage(ClientUser user, Product product)
        {
            ProductSubPage productSubPage = new ProductSubPage { DataContext = new ProductSubPageVM<RelayCommand>(user, product, new WindowsPageService()) };
            SubNavigationService.Navigate(productSubPage);
        }

        public void ShowSearchSubPage(ClientUser user)
        {
            SearchSubPage searchSubPage = new SearchSubPage { DataContext = new SearchSubPageVM<RelayCommand>(user, new WindowsPageService()) };
            SubNavigationService.Navigate(searchSubPage);
        }

        public void ShowFavoritesSubPage(ClientUser user)
        {
            OffersSubPage offersSubPage = new OffersSubPage { DataContext = new OffersSubPageVM<RelayCommand>(user, new WindowsPageService(), ClientAppResourceManager.GetString("UI_MainPage_FavoritesButton"), true) };
            SubNavigationService.Navigate(offersSubPage);
        }

        public void ShowCurrentRequestSubPage(ClientUser user)
        {
            CurrentRequestSubPage crSubPage = new CurrentRequestSubPage { DataContext = new CurrentRequestSubPageVM<RelayCommand>(user, new WindowsPageService()) };
            SubNavigationService.Navigate(crSubPage);
        }

        public void ShowArchivedRequestsListSubPage(ClientUser user)
        {
            ArchivedRequestsListSubPage archivedSubPage = new ArchivedRequestsListSubPage { DataContext = new ArchivedRequestsListSubPageVM<RelayCommand>(user, new WindowsPageService()) };
            SubNavigationService.Navigate(archivedSubPage);
        }

        public void ShowArchivedRequestSubPage(ClientUser user, ArchivedRequest request)
        {
            ArchivedRequestSubPage archivedRequestSubPage = new ArchivedRequestSubPage() { DataContext = new ArchivedRequestSubPageVM<RelayCommand>(user, request, new WindowsPageService()) };
            SubNavigationService.Navigate(archivedRequestSubPage);
        }


        private static void CanNavigate(object sender, NavigatingCancelEventArgs e)
        {
            if (SubNavigationService == null) return;
            if (SubNavigationService.Content is ProductSubPage page)
            {
                ProductSubPageVM<RelayCommand> dc = (ProductSubPageVM<RelayCommand>)page.DataContext;
                if (dc.AreChangesWereMade)
                {
                    WindowsDialogService dialogService = new WindowsDialogService();
                    if (dialogService.ShowOkCancelDialog(ClientAppResourceManager.GetString("UI_ProductSubPage_HasChangesDlgText"), ClientAppResourceManager.GetString("UI_ProductSubPage_HasChangesDlgCaption")) == false)
                        e.Cancel = true;
                }
            }
        }

        private static void OnNavigated(object sender, NavigationEventArgs e)
        {

            if (e.Content is CurrentRequestSubPage page)
            {
                CurrentRequestSubPageVM<RelayCommand> dc = (CurrentRequestSubPageVM<RelayCommand>)page.DataContext;
                dc.LoadCurrentRequestDataCommand.Execute(null);
            }
        }

        public WindowsPageService()
        {
            if (NavigationService != null)
            {
                NavigationService.Navigating -= CanNavigate;
                NavigationService.Navigating += CanNavigate;
                NavigationService.Navigated -= OnNavigated;
                NavigationService.Navigated += OnNavigated;
            }

            if (SubNavigationService != null)
            {
                SubNavigationService.Navigating -= CanNavigate;
                SubNavigationService.Navigating += CanNavigate;
                SubNavigationService.Navigated -= OnNavigated;
                SubNavigationService.Navigated += OnNavigated;
            }

        }
    }
}
