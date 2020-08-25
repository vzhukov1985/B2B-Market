using ClientApp_Mobile.ViewModels.SubPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Views.SubPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentRequestSubPage : ContentPage
    {
        public CurrentRequestSubPage()
        {
            InitializeComponent();
            BindingContext = new CurrentRequestSubPageVM();
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            typeof(Color).GetProperty("Accent", BindingFlags.Public | BindingFlags.Static).SetValue(null, Color.Transparent); //Remove Line under listview group header
            ((CurrentRequestSubPageVM)BindingContext).QueryDb(true);
            
        }
    }
}