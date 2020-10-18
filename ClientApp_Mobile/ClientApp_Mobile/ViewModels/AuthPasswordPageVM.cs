using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientApp_Mobile.Services;
using ClientApp_Mobile.Views;
using Core.DBModels;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class AuthPasswordPageVM : BaseVM
    {
        private string _login;
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged("Login");
                AuthorizeCommand.ChangeCanExecute();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public Command AuthorizeCommand { get; }

        public AuthPasswordPageVM()
        {
            AuthorizeCommand = new Command(o => Task.Run(() => Authorize()), _ => !string.IsNullOrEmpty(Login));
        }

        public void Authorize()
        {
            IsBusy = true;
            var LoginResult = ApiConnect.Login(new UserAuthParams { AuthType = AuthType.ByPassword, Login = Login, PasswordOrPin = Password });
            if (LoginResult != ApiConnect.LoginResult.Ok)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DialogService.ShowErrorDlg("Неверный логин или пароль");
                    IsBusy = false;
                });
                return;
            }

            AppSettings.CurrentUser = ApiConnect.GetUserInfo().Result;

            if (Password == AppSettings.CurrentUser.InitialPassword)
            {
                IsBusy = false;
                Device.BeginInvokeOnMainThread(() => AppPageService.GoToFirstTimePasswordSetPage());
            }
            else
            {
                if (AppSettings.AppLocalUsers.UserExistsInApp())
                {
                    AppSettings.AppLocalUsers.RegisterExistingUser();
                    IsBusy = false;
                    Device.BeginInvokeOnMainThread(() => AppPageService.GoToMainMage());
                }
                else
                {
                    bool biometricAvailable = false;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        string AuthType = DependencyService.Get<IBiometricAuthenticateService>().GetAuthenticationType();
                        if (AuthType.Equals("TouchId") || (AuthType.Equals("FaceId")))
                        {
                            biometricAvailable = true;
                        }
                    }

                    if (Device.RuntimePlatform == Device.Android)
                    {
                        bool res = DependencyService.Get<IBiometricAuthenticateService>().fingerprintEnabled();
                        if (res)
                        {
                            biometricAvailable = true;
                        }
                    }

                    IsBusy = false;
                    if (biometricAvailable && AppSettings.CurrentUser.PinHash != null)
                    {
                        Device.BeginInvokeOnMainThread(() => AppPageService.GoToNewLocalUserBiometricSettingsPage());
                    }
                    else
                    {
                        AppSettings.CurrentUser.UseBiometricAccess = false;
                        AppSettings.AppLocalUsers.RegisterNewUser();
                        Device.BeginInvokeOnMainThread(() => AppPageService.GoToMainMage());
                    }
                }
            }
        }
    }
}
