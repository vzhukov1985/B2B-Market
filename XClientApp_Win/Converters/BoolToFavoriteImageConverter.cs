using XClientApp.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace XClientApp_Win.Converters
{
    public class BoolToFavoriteImageConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] imgData = (bool)value ? XClientApp.Resources.Images.UI_Image_Favorite_On : XClientApp.Resources.Images.UI_Image_Favorite_Off;
            using (var ms = new System.IO.MemoryStream(imgData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
