using ClientApp_Mobile.ViewModels.SubPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Views.SubPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductSubPage : ContentPage
    {
        public ProductSubPage()
        {
            InitializeComponent();
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            ProductSubPageVM bc = (ProductSubPageVM)BindingContext;
            try
            {
                if (bc.IsBusy) bc.CTS.Cancel();
            }
            catch
            {
                return;
            }
        }
    }
}