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
    class UserSettingsPageVM:BaseVM
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

        private async void UpdateNameSurname()
        {
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    var clientUserRecord = ClientUser.CloneForDb(AppSettings.CurrentUser);
                    clientUserRecord.Name = Name;
                    clientUserRecord.Surname = Surname;
                    db.ClientsUsers.Update(clientUserRecord);
                    await db.SaveChangesAsync();
                }
                AppSettings.CurrentUser.Name = Name;
                AppSettings.CurrentUser.Surname = Surname;
                AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
                Device.BeginInvokeOnMainThread(() => DialogService.ShowMessageDlg("Имя и фамилия пользователя успешно обновлены", "Сохранено"));
                UpdateNameSurnameCommand.ChangeCanExecute();
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                return;
            }

        }

        private async void UpdateLogin()
        {
            try
            {
                var oldLogin = AppSettings.CurrentUser.Login;
                using (MarketDbContext db = new MarketDbContext())
                {
                    var clientUserRecord = ClientUser.CloneForDb(AppSettings.CurrentUser);
                    clientUserRecord.Login = Login;
                    db.ClientsUsers.Update(clientUserRecord);
                    await db.SaveChangesAsync();

                }
                AppSettings.CurrentUser.Login = Login;
                existingLogins[existingLogins.FindIndex(el => el == oldLogin)] = Login;
                IsLoginValid = false;
                AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
                Device.BeginInvokeOnMainThread(() => DialogService.ShowMessageDlg("Логин был успешно изменен", "Сохранено"));
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                return;
            }

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

                    try
                    {
                        using (MarketDbContext db = new MarketDbContext())
                        {
                            var clientUserRecord = ClientUser.CloneForDb(AppSettings.CurrentUser);
                            clientUserRecord.PinHash = PINHash;
                            db.ClientsUsers.Update(clientUserRecord);
                            await db.SaveChangesAsync();

                        }
                        AppSettings.CurrentUser.PinHash = PINHash;
                        UsePINAccess = true;
                        AppSettings.AppLocalUsers.CurrentUser.UsePINAccess = true;
                        AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
                    }
                    catch
                    {
                        Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                        return;
                    }
                }
                else
                {
                    DialogService.ShowErrorDlg("Введенные коды доступа не совпадают. Попробуйте еще раз.");
                }
            }
            else
            {
                try
                {
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        var clientUserRecord = ClientUser.CloneForDb(AppSettings.CurrentUser);
                        clientUserRecord.PinHash = null;
                        db.ClientsUsers.Update(clientUserRecord);
                        await db.SaveChangesAsync();

                    }
                    AppSettings.CurrentUser.PinHash = null;
                    UsePINAccess = false;
                    AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() => DialogService.ShowConnectionErrorDlg());
                    return;
                }
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
                UseBiometricAccess = await ShellPageService.GotoBiometricTestPage();
            }
            else
            {
                UseBiometricAccess = false;
            }

            AppSettings.CurrentUser.UseBiometricAccess = UseBiometricAccess;
            AppSettings.AppLocalUsers.UpdateCurrentUserPreferences();
        }

        private async void QueryDb()
        {
            IsBusy = true;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    existingLogins = await db.ClientsUsers.Select(cu => cu.Login).ToListAsync();
                }
            }
            catch
            {
                IsBusy = false;
                DialogService.ShowConnectionErrorDlg();
                return;
            }
            IsLoginValid = false;
            IsBusy = false;
        }

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

    }
}
