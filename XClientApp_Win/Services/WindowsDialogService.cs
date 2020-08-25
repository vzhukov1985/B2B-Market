using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using XClientApp.Services;
using Core.DBModels;
using System.Resources;
using System.Reflection;
using System.Windows;

namespace XClientApp_Win.Services
{
    class WindowsDialogService : IDialogService
    {

        public void ShowErrorDlg(string Text)
        {
            MessageBox.Show(Text, "Ошибка");
        }

        public bool ShowOkCancelDialog(string text, string caption)
        {
            if (MessageBox.Show(text, caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                return true;
            return false;
        }
    }
}
