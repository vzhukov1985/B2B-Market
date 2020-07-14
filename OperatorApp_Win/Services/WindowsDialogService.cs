using Core.Models;
using OperatorApp.Services;
using OperatorApp_Win.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace OperatorApp_Win.Services
{
    public class WindowsDialogService: IDialogService
    {
        public bool ShowOkCancelDialog(string text, string caption)
        {
            if (MessageBox.Show(text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                return true;
            return false;
        }

        public QuantityUnit ShowEditQuantityUnitDialog(QuantityUnit quantityUnit)
        {
            EditQuantityUnitDlg dlg = new EditQuantityUnitDlg(quantityUnit);
            if (dlg.ShowDialog() == true)
            {
                return new QuantityUnit { ShortName = dlg.ShortName.Text, FullName = dlg.FullName.Text };
            }
            return null;
        }

        public bool ShowWarningQuantityUnitDialog(string SupplierName, string MatchUnit, string QuantityUnit, string DeleteUnit)
        {
            WarningQuantityUnitDialog dlg = new WarningQuantityUnitDialog(SupplierName, MatchUnit, QuantityUnit, DeleteUnit);

            return dlg.ShowDialog() == true? true: false;
        }
    }
}
