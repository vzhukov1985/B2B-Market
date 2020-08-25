using ClientApp_Mobile.ViewModels;
using ClientApp_Mobile.ViewModels.SubPages;
using ClientApp_Mobile.Views.SubPages;
using Core.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            InitializeComponent();

            Routing.RegisterRoute("MidCategories", typeof(MidCategoriesSubPage));
            Routing.RegisterRoute("ProductCategories", typeof(ProductCategoriesSubPage));
            Routing.RegisterRoute("Offers", typeof(OffersSubPage));
            Routing.RegisterRoute("Search", typeof(SearchSubPage));
            Routing.RegisterRoute("Product", typeof(ProductSubPage));
            Routing.RegisterRoute("ProductPicture", typeof(ProductPictureSubPage));
            Routing.RegisterRoute("CurrentRequestConfirm", typeof(CurrentRequestConfirmSubPage));
            Routing.RegisterRoute("ArchivedRequest", typeof(ArchivedRequestSubPage));
        }
    }
}