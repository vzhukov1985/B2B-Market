using ClientApp_Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthPINPage : ContentPage
    {
        public AuthPINPage()
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.iOS) MessagingCenter.Subscribe<string>("iOS_Picker", "Unfocus", _ => UsersPicker.Unfocus());
        }
    }
}