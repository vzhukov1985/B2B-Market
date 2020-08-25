using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace XClientApp_Win.Converters
{
    public class RequestStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            var val = (ArchivedRequestsStatus)value;
            return val.ArchivedRequestStatusType.Description + " " + (val.ArchivedRequestStatusType.Name) switch
            {
                "PENDING" => "",
                _ => ((DateTime)val.DateTime).ToString("dd.MM.yyyy в HH:mm")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
