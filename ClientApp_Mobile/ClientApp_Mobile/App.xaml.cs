using ClientApp_Mobile.Services;
using ClientApp_Mobile.ViewModels;
using ClientApp_Mobile.ViewModels.SubPages;
using ClientApp_Mobile.Views;
using ClientApp_Mobile.Views.SubPages;
using Core.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: ResolutionGroupName("ClientApp")]
namespace ClientApp_Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            UserService.AppLocalUsers.ReadAppUserPreferences();
            
            if (UserService.AppLocalUsers.Any(lu => lu.PINAccess == true))
            {
                AppPageService.GoToAuthPINPage();
            }
            else
            {
                AppPageService.GoToAuthPasswordPage();
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
