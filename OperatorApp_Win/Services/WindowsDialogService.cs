using Core.DBModels;
using Core.Models;
using OperatorApp.DialogsViewModels;
using OperatorApp.Services;
using OperatorApp_Win.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OperatorApp_Win.Services
{
    public class WindowsDialogService : IDialogService
    {
        public bool ShowOkCancelDialog(string text, string caption)
        {
            if (MessageBox.Show(text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                return true;
            return false;
        }

        public void ShowMessageDialog(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButton.OK);
        }


        public List<ElementField> ShowAddEditElementDlg(List<ElementField> fields, bool isEditing)
        {
            AddEditElementDlg dlg = new AddEditElementDlg() { DataContext = new AddEditElementDlgVM(fields, isEditing) };
            if (dlg.ShowDialog() == true)
            {
                return ((AddEditElementDlgVM)dlg.DataContext).Fields.ToList();
            }
            return null;
        }

        public bool ShowMatchOfferDlg(MatchOffer matchOffer,
            Offer offer,
            List<ProductCategory> availableCategories,
            List<VolumeType> availableVolumeTypes,
            List<VolumeUnit> availableVolumeUnits,
            List<ProductExtraPropertyType> availableProductExtraPropertyTypes,
            List<QuantityUnit> availableQuantityUnits)
        {
            MatchOfferDlg dlg = new MatchOfferDlg() { DataContext = new MatchOfferDlgVM<RelayCommand>(matchOffer, offer, new WindowsDialogService(), availableCategories, availableVolumeTypes, availableVolumeUnits, availableProductExtraPropertyTypes, availableQuantityUnits) };

            if (dlg.ShowDialog() == true)
            {
                return true;
            }
            return false;
            
        }

        public bool ShowWarningElementsRemoveDialog(List<Tuple<string, string>> elements)
        {
            WarningElementsRemoveDlg dlg = new WarningElementsRemoveDlg() { DataContext = new WarningElementsRemoveDlgVM(elements) };
            return dlg.ShowDialog() == true;

        }
    }
}
