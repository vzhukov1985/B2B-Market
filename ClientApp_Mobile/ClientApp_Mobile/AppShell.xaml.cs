using ClientApp_Mobile.ViewModels;
using ClientApp_Mobile.ViewModels.SubPages;
using ClientApp_Mobile.Views.SubPages;
using Core.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : Shell
    {

        public static ClientUser CurrentUser;

        public AppShell()
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                CurrentUser = db.ClientsUsers
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

            InitializeComponent();

            Routing.RegisterRoute("Categories", typeof(CategoriesSubPage));
        }
    }
}