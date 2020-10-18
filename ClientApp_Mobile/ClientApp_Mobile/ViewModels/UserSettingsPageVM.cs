using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    class UserSettingsPageVM : BaseVM
    {
        public string ClientName { get; set; }
        public string Status { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                if (UpdateNameSurnameCommand != null)
                    UpdateNameSurnameCommand.ChangeCanExecute();
                OnPropertyChanged("Name");
            }
        }

        private string _Surname;
        public string Surname
        {
            get { return _Surname; }
            set
            {
                _Surname = value;
                if (UpdateNameSurnameCommand != null)
                    UpdateNameSurnameCommand.ChangeCanExecute();
                OnPropertyChanged("Surname");
            }
        }

        private string _login;
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                if (existingLogins != null)
                    IsLoginValid = !string.IsNullOrEmpty(_login) && !existingLogins.Contains(Login);
                OnPropertyChanged("Login");
            }
        }

        private bool _isLoginValid;
        public bool IsLoginValid
        {
            get { return _isLoginValid; }
            set
            {
                _isLoginValid = value;
                if (UpdateLoginCommand != null)
                    UpdateLoginCommand.ChangeCanExecute();
                OnPropertyChanged("IsLoginValid");
            }
        }


        public string PasswordHash { get; set; }

        private bool _usePINAccess;
        public bool UsePINAccess
        {
            get { return _usePINAccess; }
            set
            {
                _usePINAccess = value;
                if (ChangePINCommand != null)
                    ChangePINCommand.ChangeCanExecute();
                if (canBiometricAccessBeSet && _usePINAccess)
                    IsBiometricAccessVisible = true;
                else
                    IsBiometricAccessVisible = false;

                OnPropertyChanged("UsePINAccess");
            }
        }

        public string PINHash { get; set; }

        private bool _useBioMetricAccess;
        public bool UseBiometricAccess
        {
            get { return _useBioMetricAccess; }
            set
            {
                _useBioMetricAccess = value;
                OnPropertyChanged("UseBiometricAccess");
            }
        }

        private bool _isBiometricAccessVisible;
        public bool IsBiometricAccessVisible
        {
            get { return _isBiometricAccessVisible; }
            set
            {
                _isBiometricAccessVisible = value;
                OnPropertyChanged("IsBiometricAccessVisible");
            }
        }
        private string _biometricAccessTypeInfo;
        public string BiometricAccessTypeInfo
        {
            get { return _biometricAccessTypeInfo; }
            set
            {
                _biometricAccessTypeInfo = value;
                OnPropertyChanged("BiometricAccessTypeInfo");
            }
        }



        private bool canBiometricAccessBeSet;
        
        private List<string> existingLogins;

        public Command UpdateNameSurnameCommand { get; }
        public Command UpdateLoginCommand { get; }
        public Command ChangePasswordCommand { get; }
        public Command ChangePINAccessCommand { get; }
        public Command ChangePINCommand { get; }
        public Command ChangeBiometricAccessCommand { get; }


        public UserSettingsPageVM()
        {
            var user = AppSettings.CurrentUser;
            ClientName = user.Client.ShortName;
            Status = user.IsAdmin ? "Администратор" : "Пользователь";
            Name = user.Name;
            Surname = user.Surname;
            Login = user.Login;
            PasswordHash = user.PasswordHash;
            UsePINAccess = !string.IsNullOrEmpty(user.PinHash);
            PINHash = user.PinHash;
            UseBiometricAccess = AppSettings.AppLocalUsers.CurrentUser.UseBiometricAccess;
            existingLogins = new List<string>();

            UpdateNameSurnameCommand = new Command(_ => UpdateNameSurname(), _ => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Surname) && !(Name == AppSettings.CurrentUser.Name && Surname == AppSettings.CurrentUser.Surname));
            UpdateLoginCommand = new Command(_ => UpdateLogin(), _ => IsLoginValid);
            ChangePasswordCommand = new Command(_ => ShellPageService.GotoChangePasswordPage());
            ChangePINAccessCommand = new Command(_ => ChangePINAccess());
            ChangePINCommand = new Command(_ => { UsePINAccess = false; ChangePINAccess(); }, _ => UsePINAccess);
            ChangeBiometricAccessCommand = new Command(_ => ChangeBiometricAccess());

            CheckBiometricAccess();
            QueryDb();
        }


        private async void QueryDb()
        {
            IsBusy = true;

            existingLogins = await ApiConnect.GetAllClientsUsersLogins();
            IsLoginValid = false;

            IsBusy = false;
        }

        private async void UpdateNameSurname()
        {
            var oldName = AppSettings.CurrentUser.Name;
            var oldSurname = AppSettings.CurrentUser.Surname = Surname;
            AppSettings.CurrentUser.Name = Name;
            AppSettings.CurrentUser.Surname = Surname;
            UpdateNameSurnameCommand.ChangeCanExecute();
            if (!await ApiConnect.UpdateUserNameAndSurname(Name, Surname))
            {
                AppSettings.CurrentUser.Name = oldName;
                AppSettings.CurrentUser.Surname = oldSurname;
                UpdateNameSurnameCommand.ChangeCanExecute();
                Device.BeginInvokeOnMainThread(() => DialogService.ShowErrorDlg("Имя и фамилия пользователя не были изменены"));
                return;
            }

            AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
            Device.BeginInvokeOnMainThread(() => DialogService.ShowMessageDlg("Имя и фамилия пользователя успешно обновлены", "Сохранено"));
        }

        private async void UpdateLogin()
        {
            var oldLogin = AppSettings.CurrentUser.Login;
            AppSettings.CurrentUser.Login = Login;

            if (!await ApiConnect.UpdateUserLogin(Login))
            {
                AppSettings.CurrentUser.Login = oldLogin;
                Device.BeginInvokeOnMainThread(() => DialogService.ShowErrorDlg("Логин не был изменен"));
                return;
            }

            existingLogins[existingLogins.FindIndex(el => el == oldLogin)] = Login;
            IsLoginValid = false;
            AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
            Device.BeginInvokeOnMainThread(() => DialogService.ShowMessageDlg("Логин был успешно изменен", "Сохранено"));
        }

        private async void ChangePINAccess()
        {
            if (!UsePINAccess)
            {
                string PIN1 = await ShellPageService.GotoSetPINPage("Введите код быстрого доступа:");
                if (string.IsNullOrEmpty(PIN1)) return;
                string PIN2 = await ShellPageService.GotoSetPINPage("Повторите код быстрого доступа:");
                if (string.IsNullOrEmpty(PIN2)) return;

                if (PIN1.Equals(PIN2))
                {
                    var PINHash = Authentication.HashPIN(PIN1);

                    if (!await ApiConnect.UpdateUserPinAndPassword(AppSettings.CurrentUser.PasswordHash, PINHash))
                    {
                        DialogService.ShowErrorDlg("Код быстрого доступа не был изменен.");
                        return;
                    }

                    AppSettings.CurrentUser.PinHash = PINHash;
                    UsePINAccess = true;
                    AppSettings.AppLocalUsers.CurrentUser.UsePINAccess = true;
                    AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
                }
                else
                {
                    DialogService.ShowErrorDlg("Введенные коды доступа не совпадают. Попробуйте еще раз.");
                }
            }
            else
            {
                if (!await ApiConnect.UpdateUserPinAndPassword(AppSettings.CurrentUser.PasswordHash, null))
                {
                    DialogService.ShowErrorDlg("Параметры быстрого доступа не были изменены.");
                    return;
                }

                AppSettings.CurrentUser.PinHash = null;
                UseBiometricAccess = false;
                AppSettings.CurrentUser.UseBiometricAccess = false;
                UsePINAccess = false;
                AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
            }
        }

        private void CheckBiometricAccess()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                string AuthType = DependencyService.Get<IBiometricAuthenticateService>().GetAuthenticationType();
                if (AuthType.Equals("None") || AuthType.Equals("PassCode"))
                {
                    canBiometricAccessBeSet = false;
                }
                if (AuthType.Equals("TouchId"))
                {
                    BiometricAccessTypeInfo = "Разрешить вход с помощью Touch ID";
                    canBiometricAccessBeSet = true;
                }
                if (AuthType.Equals("FaceId"))
                {
                    BiometricAccessTypeInfo = "Разрешить вход с помощью Face ID";
                    canBiometricAccessBeSet = true;
                }
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                bool res = DependencyService.Get<IBiometricAuthenticateService>().fingerprintEnabled();
                if (res)
                {
                    BiometricAccessTypeInfo = "Разрешить вход с помощью отпечатков пальцев";
                    canBiometricAccessBeSet = true;
                }
                else
                {
                    canBiometricAccessBeSet = false;
                }
            }
        }

        private async void ChangeBiometricAccess()
        {
            if (!UseBiometricAccess)
            {
                if (Device.RuntimePlatform == Device.Android)
                    UseBiometricAccess = await ShellPageService.GotoBiometricTestPage();

                if (Device.RuntimePlatform == Device.iOS)
                    GetAuthResults();
            }
            else
            {
                UseBiometricAccess = false;
            }

            AppSettings.CurrentUser.UseBiometricAccess = UseBiometricAccess;
            AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
        }

        private async void GetAuthResults()
        {
            var result = await DependencyService.Get<IBiometricAuthenticateService>().AuthenticateUserIDWithTouchID();
            if (result)
            {
                UseBiometricAccess = true;
                AppSettings.CurrentUser.UseBiometricAccess = UseBiometricAccess;
                AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
            }
        }
    }
}
