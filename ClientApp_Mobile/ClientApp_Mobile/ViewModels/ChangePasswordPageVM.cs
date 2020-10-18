using System;
using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class ChangePasswordPageVM:BaseVM
    {
        private string _oldPassword;
        public string OldPassword
        {
            get { return _oldPassword; }
            set
            {
                _oldPassword = value;
                if (ProceedCommand != null)
                    ProceedCommand.ChangeCanExecute();
                OnPropertyChanged("OldPassword");
            }
        }

        private string _newPassword1;
        public string NewPassword1
        {
            get { return _newPassword1; }
            set
            {
                _newPassword1 = value;
                if (ProceedCommand != null)
                    ProceedCommand.ChangeCanExecute();
                OnPropertyChanged("NewPassword1");
            }
        }

        private string _newPassword2;
        public string NewPassword2
        {
            get { return _newPassword2; }
            set
            {
                _newPassword2 = value;
                if (ProceedCommand != null)
                    ProceedCommand.ChangeCanExecute();
                OnPropertyChanged("NewPassword2");
            }
        }


        public Command ProceedCommand { get; }
        public Command GoBackCommand { get; }

        public ChangePasswordPageVM()
        {
            ProceedCommand = new Command(_ => Proceed(), _ => !string.IsNullOrEmpty(NewPassword1) && !string.IsNullOrEmpty(NewPassword2) && !string.IsNullOrEmpty(OldPassword));
            GoBackCommand = new Command(_ => ShellPageService.GoBack());
        }

        private async void Proceed()
        {
            if (!Authentication.CheckPassword(OldPassword, AppSettings.CurrentUser.PasswordHash))
            {
                DialogService.ShowErrorDlg("Старый пароль введен неверно");
                return;
            }
            if (!NewPassword1.Equals(NewPassword2))
            {
                DialogService.ShowErrorDlg("Введенные пароли не совпадают.");
                return;
            }
            if (NewPassword1.Equals(AppSettings.CurrentUser.InitialPassword))
            {
                DialogService.ShowErrorDlg("Пароль не должен совпадать с начальным. Придумайте новый пароль.");
                return;
            }

            var newPasswordHash = Authentication.HashPassword(NewPassword1);

            if (!await ApiConnect.UpdateUserPinAndPassword(newPasswordHash, AppSettings.CurrentUser.PinHash))
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowErrorDlg("Пароль не был изменен"));
                return;
            }

            AppSettings.CurrentUser.PasswordHash = newPasswordHash;
            Device.BeginInvokeOnMainThread(() => { DialogService.ShowMessageDlg("Пароль был успешно изменен", "Сохранено"); ShellPageService.GoBack(); });
        }

    }
}