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
    /// Interaction logic for WarningQuantityUnitDialog.xaml
    /// </summary>
    public partial class WarningMatchUnitAndDeleteDialog : Window
    {
        public WarningMatchUnitAndDeleteDialog(string SupplierName, string MatchUnit, string QuantityUnit, string DeleteUnit)
        {
            InitializeComponent();
            SupplierAndMatchUnit.Text = $"от поставщика {SupplierName}: {MatchUnit}";
            this.QuantityUnit.Text = QuantityUnit;
            UnitToDelete.Text = DeleteUnit;
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
