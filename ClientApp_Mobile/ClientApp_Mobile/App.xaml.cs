using ClientApp_Mobile.Services;
using ClientApp_Mobile.ViewModels;
using ClientApp_Mobile.ViewModels.SubPages;
using ClientApp_Mobile.Views;
using ClientApp_Mobile.Views.SubPages;
using Core.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

            /*            using (MarketDbContext db = new MarketDbContext())
                        {
                            UserService.CurrentUser = db.ClientsUsers
                                .Where(u => u.Login == "qwe")
                                .Include(u => u.FavoriteProducts)
                                .ThenInclude(f => f.Product)
                                .Include(u => u.Client)
                                .ThenInclude(c => c.Contracts)
                                .ThenInclude(ct => ct.Supplier)
                                .Include(u => u.Client)
                                .ThenInclude(c => c.CurrentOrders)
                                .ThenInclude(o => o.Offer)
                                .ThenInclude(of => of.QuantityUnit)
                                .Include(u => u.Client)
                                .ThenInclude(c => c.CurrentOrders)
                                .ThenInclude(o => o.Offer)
                                .ThenInclude(of => of.Supplier)
                                .FirstOrDefault();
                        }

                        MainPage = new AppShell();*/

            MainPage = new AuthPasswordPage();

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
