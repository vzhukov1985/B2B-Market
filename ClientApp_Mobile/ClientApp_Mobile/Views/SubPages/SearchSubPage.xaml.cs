using ClientApp_Mobile.ViewModels.SubPages;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    public partial class SearchSubPage : ContentPage
    {
        public SearchSubPage()
        {
            InitializeComponent();
            BindingContext = new SearchSubPageVM();
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            //base.OnAppearing();
            await Task.Delay(500);
            SearchField.Focus();
        }
    }
}