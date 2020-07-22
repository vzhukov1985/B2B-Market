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
    /// Interaction logic for ProductExtraPropertyTypesPage.xaml
    /// </summary>
    public partial class ProductExtraPropertyTypesPage : Page
    {
        public ProductExtraPropertyTypesPage()
        {
            InitializeComponent();
        }
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int comboBoxItemIndex = (sender as Selector).SelectedIndex;
            CollectionViewSource cvs = FindResource("SortProductExtraPropertyTypesCVS") as CollectionViewSource;
            cvs.SortDescriptions.Clear();
            if (comboBoxItemIndex == 0)
            {
                cvs.SortDescriptions.Add(new SortDescription("SupplierProductExtraPropertyTypeName", ListSortDirection.Ascending));
            }
            else
            {
                cvs.SortDescriptions.Add(new SortDescription("Supplier.ShortName", ListSortDirection.Ascending));
            }
        }
    }
}
