using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OperatorApp_Win.Converters
{
    public class PicStatusesToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1]== DependencyProperty.UnsetValue)
                return new SolidColorBrush(Color.FromRgb(0, 128, 0));
            if ((bool)values[0] == false)
                return new SolidColorBrush(Color.FromRgb(128, 0, 0));
            if ((bool)values[1] == true)
                return new SolidColorBrush(Color.FromRgb(0, 0, 128));
            return new SolidColorBrush(Color.FromRgb(0, 128, 0));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
