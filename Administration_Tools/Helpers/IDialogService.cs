using System;
using System.Collections.Generic;
using System.Text;
using Core.Models;

namespace Administration_Tools.Helpers
{
    public interface IDialogService
    {
        Client AddContractWithClientDlg(List<Client> AvailableClients);
    }
}
