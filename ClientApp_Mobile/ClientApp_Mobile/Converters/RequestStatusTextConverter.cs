using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.Converters
{
    public class RequestStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            var val = (ArchivedRequestsStatus)value;
            string dateTime;
            switch (val.ArchivedRequestStatusType.Name)
            {
                case "PENDING": dateTime = ""; break;
                default: dateTime = ((DateTime)val.DateTime).ToString("dd.MM.yyyy в HH:mm"); break;
            };
            return val.ArchivedRequestStatusType.Description + " " + dateTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
