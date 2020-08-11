using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OperatorApp_Win.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var res = new BitmapImage();
            res.BeginInit();
            res.UriSource = (Uri)value;
            res.CacheOption = BitmapCacheOption.OnLoad;
            res.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            res.EndInit();
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
