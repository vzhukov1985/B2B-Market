using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class FirstTimePwdSetPageVM: BaseVM
    {
        private string _password1;
        public string Password1
        {
            get { return _password1; }
            set
            {
                _password1 = value;
                OnPropertyChanged("Password1");
                ProceedCommand.ChangeCanExecute();
            }
        }

        private string _password2;
        public string Password2
        {
            get { return _password2; }
            set
            {
                _password2 = value;
                OnPropertyChanged("Password2");
                ProceedCommand.ChangeCanExecute();
            }
        }

        public Command ProceedCommand { get; }



        public FirstTimePwdSetPageVM()
        {
            ProceedCommand = new Command(_ => Proceed(), _ => Password1?.Length > 0 && Password2?.Length > 0);
        }

        private void Proceed()
        {
            if (!Password1.SequenceEqual(Password2))
            {
                DialogService.ShowErrorDlg("Введенные пароли не совпадают.");
                return;
            }
            if (Password1.SequenceEqual(AppSettings.CurrentUser.InitialPassword))
            {
                DialogService.ShowErrorDlg("Пароль не должен совпадать с выданным. Придумайте новый пароль.");
                return;
            }

            AppSettings.CurrentUser.PasswordHash = Authentication.HashPassword(Password1);
            AppPageService.GoToFirstTimeSettingsPage("");
        }
    }
}
