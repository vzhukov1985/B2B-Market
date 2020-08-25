using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.Converters
{
    public class NullableBoolToContractedColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool?)value == null)
                return Color.Black;
            return (bool)value ? Application.Current.Resources["Contracted_Color"] : Application.Current.Resources["NotContracted_Color"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
