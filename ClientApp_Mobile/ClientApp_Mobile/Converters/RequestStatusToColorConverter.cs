using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using Xamarin.Forms;

namespace ClientApp_Mobile.Converters
{
    public class RequestStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)value)
            {
                case "SENT": return Application.Current.Resources["Request_Sent_Color"];
                case "PENDING": return Application.Current.Resources["Request_Pending_Color"];
                case "REJECTED": return Application.Current.Resources["Request_Rejected_Color"];
                case "ACCEPTED": return Application.Current.Resources["Request_Accepted_Color"];
                default: return new Color(0, 0, 0);
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
