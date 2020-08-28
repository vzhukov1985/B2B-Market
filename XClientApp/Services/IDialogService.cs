﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Core.DBModels;

namespace XClientApp.Services
{
    public interface IDialogService
    {
        void ShowErrorDlg(string Text);
        bool ShowOkCancelDialog(string text, string caption);
    }
}