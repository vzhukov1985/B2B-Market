using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Core.Models;

namespace ClientApp.Services
{
    public interface IDialogService
    {
        void ShowErrorDlg(string Text);
    }
}
