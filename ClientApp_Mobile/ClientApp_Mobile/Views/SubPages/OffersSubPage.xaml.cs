using ClientApp_Mobile.Services;
using ClientApp_Mobile.ViewModels;
using ClientApp_Mobile.ViewModels.SubPages;
using Core.DBModels;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Views.SubPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OffersSubPage : ContentPage
    {
        static bool FirstTimeLoad;
        public static object locker = new object();
        public static bool isGoing = false;

        public static readonly BindableProperty ShowFavoritesOnlyProp = BindableProperty.Create(nameof(ShowFavoritesOnly), typeof(bool), typeof(OffersSubPage), false);
        public bool ShowFavoritesOnly
        {
            get { return (bool)GetValue(ShowFavoritesOnlyProp); }
            set
            {
                SetValue(ShowFavoritesOnlyProp, value);
            }
        }

        public OffersSubPage()
        {
            InitializeComponent();
            BindingContext = new OffersSubPageVM();
            FirstTimeLoad = true;
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            lock (locker)
            {
                if (isGoing)
                    return;
                else
                    isGoing = true;
            }
            if (ShowFavoritesOnly)
            {
                OffersSubPageVM bc = (OffersSubPageVM)BindingContext;
                FirstTimeLoad = !await Task.Run(() => bc.QueryFavoritesOnly(FirstTimeLoad));
            }
            isGoing = false;
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            OffersSubPageVM bc = (OffersSubPageVM)BindingContext;
            if (bc.IsBusy)
                bc.CTS.Cancel();
        }
    }
}