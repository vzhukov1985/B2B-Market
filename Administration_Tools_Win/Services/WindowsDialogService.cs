using System;
using System.Collections.Generic;
using System.Text;
using Administration_Tools.Helpers;
using Administration_Tools.ViewModels;
using Administration_Tools_Win.Views;
using Core.Models;

namespace Administration_Tools_Win.Services
{
    class WindowsDialogService : IDialogService
    {
        public Client AddContractWithClientDlg(List<Client> AvailableClients)
        {
            AddContractWithClientDlg dlgAdd = new AddContractWithClientDlg();
            AddContractWithClientVM dlgAddVM = new AddContractWithClientVM(AvailableClients);
            dlgAdd.DataContext = dlgAddVM;

            if (dlgAdd.ShowDialog() == true)
                return dlgAddVM.SelectedClient;
            return null;
        }
    }
}
