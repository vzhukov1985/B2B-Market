using ClientApp_Mobile.Services;
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
    public partial class BiometricTestPage : ContentPage
    {
        public event Action<bool> PageDisapearing;

        public BiometricTestPage()
        {
            InitializeComponent();
        }
        protected override void OnDisappearing()
        {
            PageDisapearing?.Invoke(((BiometricTestPageVM)BindingContext).Result);
            if (PageDisapearing != null)
            {
                foreach (var @delegate in PageDisapearing.GetInvocationList())
                {
                    PageDisapearing -= @delegate as Action<bool>;
                }
            }
            base.OnDisappearing();
        }
    }
}