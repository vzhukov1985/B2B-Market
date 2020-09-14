using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CategoriesPage.xaml
    /// </summary>
    public partial class CategoriesPage : Page
    {
        public CategoriesPage()
        {
            InitializeComponent();
        }
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboBoxItemIndex = (sender as Selector).SelectedIndex;
            CollectionViewSource cvs = FindResource("SortProductCategoriesCVS") as CollectionViewSource;
            cvs.SortDescriptions.Clear();
            if (comboBoxItemIndex == 0)
            {
                cvs.SortDescriptions.Add(new SortDescription("SupplierProductCategoryName", ListSortDirection.Ascending));
            }
            else
            {
                cvs.SortDescriptions.Add(new SortDescription("Supplier.ShortName", ListSortDirection.Ascending));
            }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            /* using (var ms = new System.IO.MemoryStream(OperatorApp.Resources.Images.EmptyPicture))
             {
                 var image = new BitmapImage();
                 image.BeginInit();
                 image.CacheOption = BitmapCacheOption.OnLoad; // here
                 image.StreamSource = ms;
                 image.EndInit();
                 ((Image)sender).Source = image;
             }*/
          //  ((Image)sender).Source = null;
        }
    }
}
