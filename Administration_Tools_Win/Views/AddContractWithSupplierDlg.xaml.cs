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

namespace Administration_Tools_Win.Views
{
    /// <summary>
    /// Interaction logic for AddContractWithSupplierDlg.xaml
    /// </summary>
    public partial class AddContractWithSupplierDlg : Window
    {
        public AddContractWithSupplierDlg()
        {
            InitializeComponent();
        }

        private void AddContract_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
