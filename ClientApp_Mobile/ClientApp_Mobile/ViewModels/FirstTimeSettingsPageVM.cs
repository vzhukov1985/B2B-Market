using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class FirstTimeSettingsPageVM : BaseVM
    {
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

        private string _accessTypeInfo;
        public string AccessTypeInfo
        {
            get { return _accessTypeInfo; }
            set
            {
                _accessTypeInfo = value;
                OnPropertyChanged("AccessTypeInfo");
            }
        }



        private bool _canBiometricAccessBeSet;
        public bool CanBiometricAccessBeSet
        {
            get { return _canBiometricAccessBeSet; }
            set
            {
                _canBiometricAccessBeSet = value;
                OnPropertyChanged("CanFingerprintBeSet");
            }
        }


        private string _pinCode;
        public string PINCode
        {
            get { return _pinCode; }
            set
            {
                _pinCode = value;
                if (!string.IsNullOrEmpty(value))
                {
                    CanBiometricAccessBeSet = true;
                }
                else
                {
                    CanBiometricAccessBeSet = false;
                }
                OnPropertyChanged("PINCode");
            }
        }


        private bool _isBiometricAccessActivated;
        public bool IsBiometricAccessActivated
        {
            get { return _isBiometricAccessActivated; }
            set
            {
                _isBiometricAccessActivated = value;
                OnPropertyChanged("IsBiometricAccessActivated");
            }
        }

        private async void ChangePINAccess()
        {
            if (string.IsNullOrEmpty(PINCode))
            {
                string PIN1 = await AppPageService.ShowSetPinPage("Введите код быстрого доступа:");
                if (string.IsNullOrEmpty(PIN1)) return;
                string PIN2 = await AppPageService.ShowSetPinPage("Повторите код быстрого доступа:");
                if (string.IsNullOrEmpty(PIN2)) return;

                if (PIN1.Equals(PIN2))
                {
                    PINCode = PIN1;
                    AppSettings.CurrentUser.PinHash = Authentication.HashPIN(PINCode);
                    if (CanBiometricAccessBeSet)
                        IsBiometricAccessVisible = true;
                }
                else
                {
                    DialogService.ShowErrorDlg("Введенные коды доступа не совпадают. Попробуйте еще раз.");
                }
            }
            else
            {
                PINCode = "";
                AppSettings.CurrentUser.PinHash = null;
                IsBiometricAccessVisible = false;
            }
        }

        private async void ChangeBiometricAccess()
        {
            if (!IsBiometricAccessActivated)
            {
                IsBiometricAccessActivated = await AppPageService.ShowBiometricTestPage();
            }
            else
            {
                IsBiometricAccessActivated = false;
            }
            AppSettings.CurrentUser.UseBiometricAccess = IsBiometricAccessActivated;
        }

        private void CheckBiometricAccess()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                string AuthType = DependencyService.Get<IBiometricAuthenticateService>().GetAuthenticationType();
                if (AuthType.Equals("None") || AuthType.Equals("PassCode"))
                {
                    InfoText = "Для быстрого входа в приложение можно установить код быстрого доступа.";
                    CanBiometricAccessBeSet = false;
                }
                if (AuthType.Equals("TouchId"))
                {
                    InfoText = "Для быстрого входа в приложение можно установить код быстрого доступа и использовать Touch ID.";
                    AccessTypeInfo = "Разрешить вход с помощью Touch ID";
                    CanBiometricAccessBeSet = true;
                }
                if (AuthType.Equals("FaceId"))
                {
                    InfoText = "Для быстрого входа в приложение можно установить код быстрого доступа и использовать Face ID.";
                    AccessTypeInfo = "Разрешить вход с помощью Face ID";
                    CanBiometricAccessBeSet = true;
                }
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                bool res = DependencyService.Get<IBiometricAuthenticateService>().fingerprintEnabled();
                if (res)
                {
                    InfoText = "Для быстрого входа в приложение можно установить код быстрого доступа и использовать отпечатки пальцев.";
                    AccessTypeInfo = "Разрешить вход с помощью отпечатков пальцев";
                    CanBiometricAccessBeSet = true;
                }
                else
                {
                    InfoText = "Для быстрого входа в приложение можно установить код быстрого доступа.";
                    CanBiometricAccessBeSet = false;
                }
            }
        }

        private async void Proceed()
        {
            IsBusy = true;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    ClientUser userToUpdate = ClientUser.CloneForDb(AppSettings.CurrentUser);
                    db.ClientsUsers.Attach(userToUpdate);
                    db.Entry(userToUpdate).Property(e => e.PasswordHash).IsModified = true;
                    db.Entry(userToUpdate).Property(e => e.PinHash).IsModified = true;
                    await db.SaveChangesAsync();
                }
                AppSettings.AppLocalUsers.RegisterNewUser();
                IsBusy = false;
                AppPageService.GoToFirstTimeReadyPage();
            }
            catch
            {
                IsBusy = false;
                DialogService.ShowConnectionErrorDlg();
                return;
            }
        }

        public Command ChangePINAccessCommand { get; }
        public Command ChangeBiometricAccessCommand { get; }
        public Command ProceedCommand { get; }

        public FirstTimeSettingsPageVM(string PINCode)
        {
            this.PINCode = PINCode;
            IsBiometricAccessActivated = false;

            ChangePINAccessCommand = new Command(_ => ChangePINAccess());
            ChangeBiometricAccessCommand = new Command(_ => ChangeBiometricAccess());
            ProceedCommand = new Command(_ => Proceed());

            CheckBiometricAccess();
        }
    }
}




