using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OperatorApp_Win.Views
{
    /// <summary>
    /// Interaction logic for PicturesPage.xaml
    /// </summary>
    public partial class PicturesPage : Page
    {
        public PicturesPage()
        {
            InitializeComponent();
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
        /*    var image = new BitmapImage();
            using (var mem = new MemoryStream(OperatorApp.Resources.Images.EmptyPicture))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                
                image.StreamSource = mem;
                image.EndInit();
           }
            ((Image)sender).Source = image;*/
        }
    }
}
