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
    public partial class PINSetPage : ContentPage
    {
        public event Action<string> PageDisapearing;
        
        public PINSetPage()
        {
            InitializeComponent();
        }
        protected override void OnDisappearing()
        {
            PageDisapearing?.Invoke(((PINSetPageVM)BindingContext).PINCode);
            if (PageDisapearing != null)
            {
                foreach (var @delegate in PageDisapearing.GetInvocationList())
                {
                    PageDisapearing -= @delegate as Action<string>;
                }
            }
            base.OnDisappearing();
        }
    }
}