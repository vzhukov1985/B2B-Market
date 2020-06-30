using ClientApp.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ClientApp_Win.Converters
{
    public class BoolToGroupingButtonContentConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ClientAppResourceManager.GetString("UI_RequestSubPage_GroupingByCategories") : ClientAppResourceManager.GetString("UI_RequestSubPage_GroupingBySuppliers");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
