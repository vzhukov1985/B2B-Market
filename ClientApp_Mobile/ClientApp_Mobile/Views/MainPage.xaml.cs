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
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : Shell
    {

        public MainPage()
        {
            InitializeComponent();

            Routing.RegisterRoute("MidCategories", typeof(CategoriesSubPage));
            Routing.RegisterRoute("ProductCategories", typeof(CategoriesSubPage));
            Routing.RegisterRoute("Offers", typeof(OffersSubPage));
            Routing.RegisterRoute("Search", typeof(SearchSubPage));
            Routing.RegisterRoute("Product", typeof(ProductSubPage));
            Routing.RegisterRoute("ProductPicture", typeof(ProductPictureSubPage));
            Routing.RegisterRoute("CurrentRequestConfirm", typeof(CurrentRequestConfirmSubPage));
            Routing.RegisterRoute("ArchivedRequest", typeof(ArchivedRequestSubPage));
        }
    }
}