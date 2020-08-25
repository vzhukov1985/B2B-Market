using ClientApp_Mobile.ViewModels.SubPages;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    public partial class ArchivedRequestsListSubPage : ContentPage
    {
        public ArchivedRequestsListSubPage()
        {
            InitializeComponent();
            BindingContext = new ArchivedRequestsListSubPageVM();
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            typeof(Color).GetProperty("Accent", BindingFlags.Public | BindingFlags.Static).SetValue(null, Color.Transparent); //Remove Line under listview group header
            ((ArchivedRequestsListSubPageVM)BindingContext).QueryDb();
        }
    }
}