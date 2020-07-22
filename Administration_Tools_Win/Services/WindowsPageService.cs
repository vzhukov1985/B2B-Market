using Administration_Tools.Services;
using Administration_Tools_Win.Views;
using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Administration_Tools.ViewModels;
using System.Windows.Navigation;

namespace Administration_Tools_Win.Services
{
    public class WindowsPageService: IPageService
    {
        public static NavigationService MainNavigationService;

        public void ShowSuppliersPage()
        {
            MainNavigationService.Navigate(new Suppliers());
        }

        public void ShowClientsPage()
        {
            MainNavigationService.Navigate(new Clients());
        }

        public void ShowClientUsersPage(Client selectedClient, IDialogService dialogService)
        {
            ClientUsers ClientUsersPage = new ClientUsers
            {
                DataContext = new ClientUsersVM<RelayCommand>(selectedClient, dialogService)
            };
            MainNavigationService.Navigate(ClientUsersPage);
        }
    }
}
