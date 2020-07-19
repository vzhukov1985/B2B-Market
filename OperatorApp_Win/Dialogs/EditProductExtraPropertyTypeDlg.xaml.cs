using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OperatorApp_Win.Dialogs
{
    /// <summary>
    /// Interaction logic for EditProductExtraPropertyTypeDlg.xaml
    /// </summary>
    public partial class EditProductExtraPropertyTypeDlg : Window
    {
        public EditProductExtraPropertyTypeDlg(ProductExtraPropertyType productExtraPropertyType)
        {
            InitializeComponent();
            EPTName.Text = productExtraPropertyType.Name;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
