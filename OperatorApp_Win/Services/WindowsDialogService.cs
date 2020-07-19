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
        public VolumeType ShowEditVolumeTypeDialog(VolumeType volumeType)
        {
            EditVolumeTypeDlg dlg = new EditVolumeTypeDlg(volumeType);
            if (dlg.ShowDialog() == true)
            {
                return new VolumeType { Name = dlg.VTName.Text };
            }
            return null;
        }
        public VolumeUnit ShowEditVolumeUnitDialog(VolumeUnit volumeUnit)
        {
            EditVolumeUnitDlg dlg = new EditVolumeUnitDlg(volumeUnit);
            if (dlg.ShowDialog() == true)
            {
                return new VolumeUnit { ShortName = dlg.ShortName.Text, FullName = dlg.FullName.Text };
            }
            return null;
        }

        public ProductExtraPropertyType ShowEditProductExtraPropertyTypeDialog(ProductExtraPropertyType productExtraPropertyType)
        {
            EditProductExtraPropertyTypeDlg dlg = new EditProductExtraPropertyTypeDlg(productExtraPropertyType);
            if (dlg.ShowDialog() == true)
            {
                return new ProductExtraPropertyType { Name = dlg.EPTName.Text };
            }
            return null;
        }

        public bool ShowWarningMatchAndDeleteDialog(string SupplierName, string MatchUnit, string QuantityUnit, string DeleteUnit)
        {
            WarningMatchUnitAndDeleteDialog dlg = new WarningMatchUnitAndDeleteDialog(SupplierName, MatchUnit, QuantityUnit, DeleteUnit);

            return dlg.ShowDialog() == true;
        }
    }
}
