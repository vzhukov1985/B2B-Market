using Core.DBModels;
using Core.Models;
using Microsoft.Win32;
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
        public string FilePath { get; set; }

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

        public string ShowPositionOffers(MatchQuantityUnit qu)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(qu);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }
        public string ShowPositionOffers(QuantityUnit qu)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(qu);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }
        public string ShowPositionOffers(MatchVolumeType vt)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(vt);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }
        public string ShowPositionOffers(VolumeType vt)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(vt);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }
        public string ShowPositionOffers(MatchVolumeUnit vu)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(vu);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }
        public string ShowPositionOffers(VolumeUnit vu)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(vu);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }
        public string ShowPositionOffers(MatchProductExtraPropertyType pept)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(pept);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }
        public string ShowPositionOffers(ProductExtraPropertyType pept)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(pept);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }
        public string ShowPositionOffers(MatchProductCategory pc)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(pc);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }
        public string ShowPositionOffers(ProductCategory pc)
        {
            var dc = new PositionDependenciesDlgVM<RelayCommand>(pc);
            PositionDependenciesDlg dlg = new PositionDependenciesDlg() { DataContext = dc };
            if (dlg.ShowDialog() == true)
                return dc.SelectedPositionName;
            else
                return null;
        }

        
        public bool ShowWarningElementsRemoveDialog(List<Tuple<string, string>> elements)
        {
            WarningElementsRemoveDlg dlg = new WarningElementsRemoveDlg() { DataContext = new WarningElementsRemoveDlgVM(elements) };
            return dlg.ShowDialog() == true;

        }

        public bool ShowOpenPictureDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Images (*.bmp, *.gif, *.exif, *.jpg, *.png, *.tiff) | *.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                return true;
            }
            return false;
        }

    }
}
