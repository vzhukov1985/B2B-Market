using System;
using System.Globalization;
using Xamarin.Forms;

namespace ClientApp_Mobile.Converters
{
    public class ArchivedOrderStatusNameToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch ((string) value)
                {
                    case "PENDING":
                        return ImageSource.FromFile("Request_Pending");
                    case "REJECTED":
                        return ImageSource.FromFile("Request_Rejected");
                    case "ACCEPTED":
                        return ImageSource.FromFile("Request_Accepted");
                    default:
                        return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
