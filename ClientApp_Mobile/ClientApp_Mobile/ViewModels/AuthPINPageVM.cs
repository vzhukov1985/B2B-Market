﻿using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class AuthPINPageVM : BaseVM
    {
        private bool _pinIsWrong;
        public bool PINIsWrong
        {
            get { return _pinIsWrong; }
            set
            {
                _pinIsWrong = value;
                OnPropertyChanged("PINIsWrong");
            }
        }

        private string _infoText;
        public string InfoText
        {
            get { return _infoText; }
            set
            {
                _infoText = value;
                OnPropertyChanged("InfoText");
            }
        }


        private ObservableCollection<AppLocalUser> _users;
        public ObservableCollection<AppLocalUser> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged("Users");
            }
        }

        private AppLocalUser _selectedUser;
        public AppLocalUser SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                OnPropertyChanged("SelectedUser");
                if (value != null && !IsBiometricCheckActive) CheckBiometricAccess();
                if (Device.RuntimePlatform == Device.iOS) MessagingCenter.Send("iOS_Picker", "Unfocus");
            }
        }

        private ImageSource _biometricImage;
        public ImageSource BiometricImage
        {
            get { return _biometricImage; }
            set
            {
                _biometricImage = value;
                OnPropertyChanged("BiometricImage");
            }
        }


        private string _pinCode;
        public string PINCode
        {
            get { return _pinCode; }
            set
            {
                _pinCode = value;
                OnPropertyChanged("PINCode");
            }
        }

        public Command PINButtonTapCommand { get; }
        public Command AuthorizeByPasswordCommand { get; }
        public Command RemoveLocalUserCommand { get; }
        public Command ForceBiometricCheckCommand { get; }

        private bool IsBiometricCheckActive;

        public AuthPINPageVM()
        {
            Users = new ObservableCollection<AppLocalUser>(AppSettings.AppLocalUsers);
            var userFromSettings = Users[AppSettings.AppLocalUsers.LastEnterUserIndex];
            Users.RemoveAll(u => u.UsePINAccess == false);
            if (Users.Contains(userFromSettings))
            {
                SelectedUser = Users.Where(u => u.Login == userFromSettings.Login).FirstOrDefault();
            }
            else
            {
                SelectedUser = Users[0];
            }

            PINIsWrong = false;

            PINButtonTapCommand = new Command<string>(s => AddPINNumber(s));
            AuthorizeByPasswordCommand = new Command(_ => { MessagingCenter.Send("AndroidAuth", "Cancel"); AppPageService.GoToAuthPasswordPage(); });
            RemoveLocalUserCommand = new Command(_ => RemoveLocalUser());
            ForceBiometricCheckCommand = new Command(_ => CheckBiometricAccess());
        }

        private void AuthorizePIN()
        {
            IsBusy = true;
            var loginResult = ApiConnect.Login(new UserAuthParams { AuthType = AuthType.ByPIN, Login = SelectedUser.Login, PasswordOrPin =  PINCode});
            if (loginResult == ApiConnect.LoginResult.Invalid)
            {
                IsBusy = false;
                Device.BeginInvokeOnMainThread(() => { PINIsWrong = true; PINIsWrong = false; });
                return;
            }

            if (loginResult == ApiConnect.LoginResult.PinAccessDenied)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    AppSettings.AppLocalUsers.RemoveAppUser(SelectedUser);
                    Users.Remove(SelectedUser);
                    if (Users.Count > 0)
                    {
                        SelectedUser = Users[0];
                    }
                    else
                    {
                        AppPageService.GoToAuthPasswordPage();
                    }
                    DialogService.ShowErrorDlg("Код быстрого доступа был сброшен на другом устройстве. Вход для этого пользователя возможен только по паролю");
                });
            }

            Task.Run(() => ProceedAuth());
            
        }

        private void ProceedAuth()
        {
            IsBusy = true;
            MessagingCenter.Send("AndroidAuth", "Cancel");
            AppSettings.CurrentUser = ApiConnect.GetUserInfo().Result;
            AppSettings.AppLocalUsers.RegisterExistingUser();
            Device.BeginInvokeOnMainThread(() => AppPageService.GoToMainMage());
            IsBusy = false;
        }

        private void ProceedBiometricAuth()
        {
            IsBusy = true;
            ApiConnect.Login(new UserAuthParams { AuthType = AuthType.ByBiometric, Login = SelectedUser.Login });
            ProceedAuth();
        }

        private void CheckBiometricAccess()
        {
            MessagingCenter.Send("AndroidAuth", "Cancel");
            if (SelectedUser.UseBiometricAccess == false)
            {
                InfoText = "Введите код:";
                BiometricImage = ImageSource.FromFile("Fingerprint_Inactive.png");
                return;
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                string authType = DependencyService.Get<IBiometricAuthenticateService>().GetAuthenticationType();
                if (authType.Equals("None") || authType.Equals("PassCode"))
                {
                    AppSettings.AppLocalUsers.DisableAllUsersBiometricAccess();
                    BiometricImage = ImageSource.FromFile("Fingerprint_Inactive.png");
                    return;
                }
                if (authType.Equals("TouchId"))
                {
                    InfoText = "Введите код или используйте Touch ID";
                    BiometricImage = ImageSource.FromFile("Fingerprint.png");
                }
                if (authType.Equals("FaceId"))
                {
                    InfoText = "Введите код или используйте Face ID";
                    BiometricImage = ImageSource.FromFile("FaceID.png");
                }
                IsBiometricCheckActive = true;
                GetAuthResults(authType);
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                bool res = DependencyService.Get<IBiometricAuthenticateService>().fingerprintEnabled();
                if (res)
                {
                    InfoText = "Введите код или используйте сканер отпечатков пальцев";
                    BiometricImage = ImageSource.FromFile("Fingerprint.png");

                    if (DependencyService.Get<IBiometricPieAuthenticate>().CheckSDKGreater29())
                    {
                        MessagingCenter.Unsubscribe<object>("AndroidAuth", "Success");
                        MessagingCenter.Subscribe<object>("AndroidAuth", "Success", (sender) =>
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                InfoText = "Отпечаток распознан. Выполняется вход...";
                                Task.Run(() => ProceedBiometricAuth());
                            });
                        });
                        MessagingCenter.Unsubscribe<object>("AndroidAuth", "Fail");
                        MessagingCenter.Subscribe<object>("AndroidAuth", "Fail", (sender) =>
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                InfoText = "Отпечаток не распознан. Попробуйте еще раз или введите код:";
                                PINIsWrong = true; PINIsWrong = false;
                            });
                        });

                        DependencyService.Get<IBiometricPieAuthenticate>().RegisterOrAuthenticate();
                    }
                    else
                    {
                        DependencyService.Get<IBiometricAuthenticateService>().AuthenticateUserIDWithTouchID();

                        MessagingCenter.Unsubscribe<string>("AndroidAuth", "Success");
                        MessagingCenter.Subscribe<string>("AndroidAuth", "Success", (sender) =>
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {

                                InfoText = "Отпечаток распознан. Выполняется вход...";
                                Task.Run(() => ProceedBiometricAuth());
                            });
                        });
                        MessagingCenter.Unsubscribe<string>("AndroidAuth", "Fail");
                        MessagingCenter.Subscribe<string>("AndroidAuth", "Fail", (sender) =>
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                InfoText = "Отпечаток не распознан. Попробуйте еще раз или введите код:";
                                PINIsWrong = true; PINIsWrong = false;
                            });
                        });
                    }
                }
                else
                {
                    AppSettings.AppLocalUsers.DisableAllUsersBiometricAccess();
                    BiometricImage = ImageSource.FromFile("Fingerprint_Inactive.png");
                }
            }
        }

        private async void GetAuthResults(string authType)
        {
            var result = await DependencyService.Get<IBiometricAuthenticateService>().AuthenticateUserIDWithTouchID();
            IsBiometricCheckActive = false;
            if (result)
            {
                InfoText = authType == "TouchId" ? "Отпечаток распознан. Выполняется вход..." : "Лицо распознано. Выполняется вход...";
                await Task.Run(() => ProceedBiometricAuth());
            }
            else
            {
                InfoText = authType == "TouchId" ? "Отпечаток не распознан. Попробуйте еще раз или введите код:" : "Лицо не распознано. Попробуйте еще раз или введите код:";
                Device.BeginInvokeOnMainThread(() => { PINIsWrong = true; PINIsWrong = false; });
            }
        }

        public async void RemoveLocalUser()
        {
            if (await DialogService.ShowOkCancelDialog($"Пользователь \"{SelectedUser.DisplayName}\" будет удален из списка быстрого доступа для этого устройства. Вы хотите продолжить?", "ВНИМАНИЕ!") == true)
            {
                AppSettings.AppLocalUsers.RemoveAppUser(SelectedUser);
                Users.Remove(SelectedUser);
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (Users.Count == 0)
                    {
                        AppPageService.GoToAuthPasswordPage();
                    }
                    else
                    {
                        SelectedUser = AppSettings.AppLocalUsers[0];
                    }
                });
            }
        }

        private void AddPINNumber(string stringToAdd)
        {
            if (stringToAdd == "Backspace")
            {
                if (PINCode.Length > 0)
                    PINCode = PINCode.Remove(PINCode.Length - 1);
            }
            else
            {
                PINCode += stringToAdd;
            }
            if (PINCode.Length == 4) Task.Run(() => AuthorizePIN());
        }
    }
}
