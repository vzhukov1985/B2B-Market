using ClientApp_Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthPasswordPage : ContentPage
    {
        public AuthPasswordPage()
        {
            InitializeComponent();
            BindingContext = new AuthPasswordPageVM();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            AuthPasswordPageVM bc = (AuthPasswordPageVM)BindingContext;
            bc.IsBusy = true;
            Task.Run(() => bc.Authorize());
        }
    }
}