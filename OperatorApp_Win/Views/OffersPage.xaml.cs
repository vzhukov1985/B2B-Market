using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for OffersPage.xaml
    /// </summary>
    public partial class OffersPage : Page
    {
        public OffersPage()
        {
            InitializeComponent();
        }
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboBoxItemIndex = (sender as Selector).SelectedIndex;
            CollectionViewSource cvs = FindResource("SortOffersCVS") as CollectionViewSource;
            cvs.SortDescriptions.Clear();
            if (comboBoxItemIndex == 0)
            {
                cvs.SortDescriptions.Add(new SortDescription("ProductName", ListSortDirection.Ascending));
            }
            else
            {
                cvs.SortDescriptions.Add(new SortDescription("Supplier.ShortName", ListSortDirection.Ascending));
            }
        }
        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var image = new BitmapImage();
            using (var mem = new MemoryStream(OperatorApp.Resources.Images.EmptyPicture))
            {
                mem.Position = 0;
                image.BeginInit();
              //  image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            ((Image)sender).Source = image;
        }
    }
}
