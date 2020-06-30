using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ClientApp_Win.Converters
{
    public class RequestStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value) switch
            {
                "SENT" => new SolidColorBrush(Color.FromRgb(103, 181, 28)),
                "PENDING" => new SolidColorBrush(Color.FromRgb(11, 0, 201)),
                "REJECTED" => new SolidColorBrush(Color.FromRgb(190, 0, 0)),
                "ACCEPTED" => new SolidColorBrush(Color.FromRgb(103, 181, 28)),
                _ => new SolidColorBrush(Color.FromRgb(0, 0, 0)),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
