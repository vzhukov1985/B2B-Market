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
        public static object locker = new object();
        public static bool isGoing = false;
        public CurrentRequestSubPage()
        {
            InitializeComponent();
            BindingContext = new CurrentRequestSubPageVM();
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            lock (locker)
            {
                if (isGoing)
                    return;
                else
                    isGoing = true;
            }
            typeof(Color).GetProperty("Accent", BindingFlags.Public | BindingFlags.Static).SetValue(null, Color.Transparent); //Remove Line under listview group header
            Task.Run(() => ((CurrentRequestSubPageVM)BindingContext).QueryDb());
            isGoing = false;
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            CurrentRequestSubPageVM bc = (CurrentRequestSubPageVM)BindingContext;
            if (bc.IsBusy) bc.CTS.Cancel();

        }
    }
}