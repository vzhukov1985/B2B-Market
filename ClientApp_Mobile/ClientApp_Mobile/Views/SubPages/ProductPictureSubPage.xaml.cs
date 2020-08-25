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
    public partial class ProductPictureSubPage : ContentPage
    {
        public ProductPictureSubPage()
        {
            InitializeComponent();
            BindingContext = new ProductPictureSubPageVM();
        }
    }
}