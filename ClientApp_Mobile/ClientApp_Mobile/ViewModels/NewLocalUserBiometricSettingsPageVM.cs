using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class NewLocalUserBiometricSettingsPageVM : BaseVM
    {
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
                if (AuthType.Equals("TouchId"))
                {
                    InfoText = "У вас установлен код быстрого доступа к приложению. Вы также можете использовать Touch ID для входа на этом устройстве.";
                    AccessTypeInfo = "Разрешить вход с помощью Touch ID";
                }
                if (AuthType.Equals("FaceId"))
                {
                    InfoText = "У вас установлен код быстрого доступа к приложению. Вы также можете использовать Face ID для входа на этом устройстве.";
                    AccessTypeInfo = "Разрешить вход с помощью Face ID";
                }
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                InfoText = "У вас установлен код быстрого доступа к приложению. Вы также можете использовать сканер отпечатков пальцев для входа на этом устройстве.";
                AccessTypeInfo = "Разрешить вход с помощью отпечатков пальцев";
            }
        }

        private void Proceed()
        {
            AppSettings.AppLocalUsers.RegisterNewUser();
            AppPageService.GoToMainMage();
        }

        public Command ChangeBiometricAccessCommand { get; }
        public Command ProceedCommand { get; }

        public NewLocalUserBiometricSettingsPageVM()
        {
            IsBiometricAccessActivated = false;

            ChangeBiometricAccessCommand = new Command(_ => ChangeBiometricAccess());
            ProceedCommand = new Command(_ => Proceed());

            CheckBiometricAccess();
        }
    }
}
