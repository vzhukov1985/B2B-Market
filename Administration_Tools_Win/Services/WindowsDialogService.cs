using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Administration_Tools.Services;
using Administration_Tools.ViewModels;
using Administration_Tools_Win.Views;
using Core.Models;
using System.Resources;
using System.Reflection;

namespace Administration_Tools_Win.Services
{
    class WindowsDialogService : IDialogService
    {
        public Client AddContractWithClientDlg(ObservableCollection<Client> AvailableClients)
        {
            AddContractWithClientDlg dlgAdd = new AddContractWithClientDlg();
            AddContractWithClientVM dlgAddVM = new AddContractWithClientVM(AvailableClients);
            dlgAdd.DataContext = dlgAddVM;

            if (dlgAdd.ShowDialog() == true)
                return dlgAddVM.SelectedClient;
            return null;
        }

        public Supplier AddContractWithSupplierDlg(ObservableCollection<Supplier> AvailableSuppliers)
        {
            AddContractWithSupplierDlg dlgAdd = new AddContractWithSupplierDlg();
            AddContractWithSupplierVM dlgAddVM = new AddContractWithSupplierVM(AvailableSuppliers);
            dlgAdd.DataContext = dlgAddVM;

            if (dlgAdd.ShowDialog() == true)
                return dlgAddVM.SelectedSupplier;
            return null;
        }

        public void ShowErrorDlg(string Text)
        {
            ResourceManager rm = new ResourceManager("Administration_Tools.Resources.UILang", typeof(IDialogService).Assembly);
            System.Windows.MessageBox.Show(Text, rm.GetString("UI_ErrorDlg_Caption"));
        }
    }
}
