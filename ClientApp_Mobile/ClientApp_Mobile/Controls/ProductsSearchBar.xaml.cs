using ClientApp_Mobile.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductsSearchBar : ContentView
    {
        public ProductsSearchBar()
        {
            InitializeComponent();
        }
        private void NoLineSearchBar_Focused(object sender, FocusEventArgs e)
        {
            ShellPageService.GoToSearchPage();
        }
    }
}