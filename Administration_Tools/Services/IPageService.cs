using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Administration_Tools.Services
{
    public interface IPageService
    {
        void ShowSuppliersPage();
        void ShowClientsPage();
        void ShowClientUsersPage(Client selectedClient, IDialogService dialogService);
    }
}
