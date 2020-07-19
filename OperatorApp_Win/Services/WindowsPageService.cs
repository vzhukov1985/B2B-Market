using OperatorApp.Services;
using OperatorApp.ViewModels;
using OperatorApp_Win.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;

namespace OperatorApp_Win.Services
{
    public class WindowsPageService: IPageService
    {
        public static NavigationService NavigationService;

        public void ShowQuantityUnitsPage()
        {
            QuantityUnitsPage quantityUnitsPage = new QuantityUnitsPage() { DataContext = new QuantityUnitsPageVM<RelayCommand>(new WindowsPageService(), new WindowsDialogService()) };
            NavigationService.Navigate(quantityUnitsPage);
        }

        public void ShowVolumeTypesPage()
        {
            VolumeTypesPage volumeTypesPage = new VolumeTypesPage() { DataContext = new VolumeTypesPageVM<RelayCommand>(new WindowsPageService(), new WindowsDialogService()) };
            NavigationService.Navigate(volumeTypesPage);
        }

        public void ShowVolumeUnitsPage()
        {
            VolumeUnitsPage volumeUnitsPage = new VolumeUnitsPage() { DataContext = new VolumeUnitsPageVM<RelayCommand>(new WindowsPageService(), new WindowsDialogService()) };
            NavigationService.Navigate(volumeUnitsPage);
        }
        
                    public void ShowProductExtraPropertyTypesPage()
        {
            ProductExtraPropertyTypesPage productExtraPropertyTypesPage = new ProductExtraPropertyTypesPage() { DataContext = new ProductExtraPropertyTypesPageVM<RelayCommand>(new WindowsPageService(), new WindowsDialogService()) };
            NavigationService.Navigate(productExtraPropertyTypesPage);
        }
    }
}
