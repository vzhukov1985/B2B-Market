using ClientApp_Mobile.Controls;
using ClientApp_Mobile.ViewModels.SubPages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ClientApp_Mobile.Views.SubPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProductSubPage : ContentPage
    {
        public ProductSubPage()
        {
            InitializeComponent();
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            ProductSubPageVM bc = (ProductSubPageVM)BindingContext;
            try
            {
                if (bc.IsBusy) bc.CTS.Cancel();
            }
            catch
            {
                return;
            }
        }

        private void DecimalEntry_Completed(object sender, EventArgs e)
        {
            var entry = (DecimalEntry)sender;
            OfferWithOrderView data = (OfferWithOrderView)entry.BindingContext;
            var strValue = entry.Text;
            if (!string.IsNullOrEmpty(strValue))
            {
                var cultureInfo = CultureInfo.InvariantCulture;
                // if the first regex matches, the number string is in us culture
                if (Regex.IsMatch(strValue, @"^(:?[\d,]+\.)*\d+$"))
                {
                    cultureInfo = new CultureInfo("en-US");
                }
                // if the second regex matches, the number string is in de culture
                else if (Regex.IsMatch(strValue, @"^(:?[\d.]+,)*\d+$"))
                {
                    cultureInfo = new CultureInfo("de-DE");
                }
                NumberStyles styles = NumberStyles.Number;
                if (decimal.TryParse(strValue, styles, cultureInfo, out decimal res))
                {
                    data.OrderQuantity = res;
                    return;
                }
            }
            data.OrderQuantity = 0;
        }
    }
}