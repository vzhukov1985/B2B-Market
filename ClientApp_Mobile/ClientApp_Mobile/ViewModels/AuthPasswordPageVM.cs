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

        public void Authorize()
        {
            IsBusy = true;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    ClientUser user = db.ClientsUsers.Where(o => o.Login == Login).FirstOrDefault();
                    if ((user != null) && (Authentication.CheckPassword(Password, user.PasswordHash)))
                    {

                        UserService.GetUserInfoFromDb(user.Id);


                        if (Password == user.InitialPassword)
                        {
                            IsBusy = false;
                            Device.BeginInvokeOnMainThread(() => AppPageService.GoToFirstTimePasswordSetPage());
                        }
                        else
                        {
                            if (UserService.AppLocalUsers.UserExistsInApp())
                            {
                                UserService.AppLocalUsers.RegisterExistingUser();
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
                                if (biometricAvailable)
                                {
                                    Device.BeginInvokeOnMainThread(() => AppPageService.GoToNewLocalUserBiometricSettingsPage());
                                }
                                else
                                {
                                    UserService.CurrentUser.UseBiometricAccess = false;
                                    UserService.AppLocalUsers.RegisterNewUser();
                                    Device.BeginInvokeOnMainThread(() => AppPageService.GoToMainMage());
                                }
                            }
                        }
                    }
                    else
                    {
                        IsBusy = false;
                        Device.BeginInvokeOnMainThread(() => DialogService.ShowErrorDlg("Неверные имя пользователя или пароль. Попробуйте снова."));
                    }
                }

            }
            catch/*(Exception e)*/
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    IsBusy = false;
                    DialogService.ShowConnectionErrorDlg();
                    // ShellDialogService.ShowMessageDlg(e.Message, "exc");
                });
                return;
            }
        }

        public Command AuthorizeCommand { get; }

        public AuthPasswordPageVM()
        {
            AuthorizeCommand = new Command(o => Task.Run(() => Authorize()), _ => !string.IsNullOrEmpty(Login));
        }
    }
}
