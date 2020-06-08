using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Core.Models;

namespace Administration_Tools.Services
{
    public interface IDialogService
    {
        Client AddContractWithClientDlg(ObservableCollection<Client> AvailableClients);
        Supplier AddContractWithSupplierDlg(ObservableCollection<Supplier> AvailableSuppliers);

        void ShowErrorDlg(string Text);
    }
}
