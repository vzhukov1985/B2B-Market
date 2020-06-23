using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ClientApp.Services;
using Core.Models;
using System.Resources;
using System.Reflection;

namespace ClientApp_Win.Services
{
    class WindowsDialogService : IDialogService
    {

        public void ShowErrorDlg(string Text)
        {
            System.Windows.MessageBox.Show(Text, ClientAppResourceManager.GetString("UI_ErrorDlg_Caption"));
        }
    }
}
