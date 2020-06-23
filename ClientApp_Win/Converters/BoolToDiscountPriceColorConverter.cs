﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ClientApp_Win.Converters
{
    public class BoolToDiscountPriceColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
                return (bool)value ? new SolidColorBrush(Color.FromRgb(0, 128, 0)) : new SolidColorBrush(Color.FromRgb(128, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
