using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;
using ClientApp.Services;
using ClientApp_Win.Views;
using ClientApp.ViewModels;

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
            FirstLoginPage firstLoginPage = new FirstLoginPage();
            firstLoginPage.DataContext = new FirstLoginPageVM<RelayCommand>(user, new WindowsDialogService(), new WindowsPageService());
            NavigationService.Navigate(firstLoginPage);
        }

        public void ShowMainPage(ClientUser user)
        {
            MainPage mainPage = new MainPage();
            mainPage.DataContext = new MainPageVM<RelayCommand>(user, new WindowsDialogService(), new WindowsPageService());
            NavigationService.Navigate(mainPage);
            ShowOffersSubPage(user);
        }

        public void ShowOffersSubPage(ClientUser user)
        {
            OffersSubPage offersSubPage = new OffersSubPage();
            offersSubPage.DataContext = new OffersSubPageVM<RelayCommand>(user);
            SubNavigationService.Navigate(offersSubPage);
        }
    }
}
