using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ClientApp.Services;
using Core.Models;
using System.Resources;
using System.Reflection;
using System.Windows;

namespace ClientApp_Win.Services
{
    class WindowsDialogService : IDialogService
    {

        public void ShowErrorDlg(string Text)
        {
            MessageBox.Show(Text, ClientAppResourceManager.GetString("UI_ErrorDlg_Caption"));
        }

        public bool ShowOkCancelDialog(string text, string caption)
        {
            if (MessageBox.Show(text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                return true;
            return false;
        }
    }
}
