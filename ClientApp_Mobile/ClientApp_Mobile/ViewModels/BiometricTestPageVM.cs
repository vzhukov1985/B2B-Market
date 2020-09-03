using ClientApp_Mobile.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class BiometricTestPageVM : BaseVM
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

        private ImageSource _bioImage;
        public ImageSource BioImage
        {
            get { return _bioImage; }
            set
            {
                _bioImage = value;
                OnPropertyChanged("BioImage");
            }
        }

        private string authType;

        public bool Result { get; set; }

        private void CheckBiometricTypes()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                authType = DependencyService.Get<IBiometricAuthenticateService>().GetAuthenticationType();
                if (authType.Equals("TouchId"))
                {
                    InfoText = "Приложите палец к сенсору отпечатков пальцев Touch ID";
                    BioImage = ImageSource.FromFile("Fingerprint.png");
                }
                if (authType.Equals("FaceId"))
                {
                    InfoText = "Посмотрите в камеру для распознования лица Face ID";
                    BioImage = ImageSource.FromFile("FaceID.png");
                }
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                InfoText = "Приложите палец к сенсору отпечатков пальцев.";
                BioImage = ImageSource.FromFile("Fingerprint.png");
            }
        }

        public void StartBiometricCheck()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                GetAuthResults();
            }
            if (Device.RuntimePlatform == Device.Android)
            {
                if (DependencyService.Get<IBiometricPieAuthenticate>().CheckSDKGreater29())
                {
                    MessagingCenter.Unsubscribe<object>("AndroidAuth", "Success");
                    MessagingCenter.Subscribe<object>("AndroidAuth", "Success", (sender) =>
                    {
                        Proceed(true);
                    });
                    MessagingCenter.Unsubscribe<object>("AndroidAuth", "Fail");
                    MessagingCenter.Subscribe<object>("AndroidAuth", "Fail", (sender) =>
                    {
                        InfoText = "Отпечаток не распознан. Попробуйте еще раз.";
                    });

                    DependencyService.Get<IBiometricPieAuthenticate>().RegisterOrAuthenticate();
                }
                else
                {
                    DependencyService.Get<IBiometricAuthenticateService>().AuthenticateUserIDWithTouchID();

                    MessagingCenter.Unsubscribe<string>("AndroidAuth", "Success");
                    MessagingCenter.Subscribe<string>("AndroidAuth", "Success", (sender) =>
                    {
                        Proceed(true);
                    });
                    MessagingCenter.Unsubscribe<string>("AndroidAuth", "Fail");
                    MessagingCenter.Subscribe<string>("AndroidAuth", "Fail", (sender) =>
                    {
                        InfoText = "Отпечаток не распознан. Попробуйте еще раз.";
                    });
                }
            }
        }

        private async void GetAuthResults()
        {
            var result = await DependencyService.Get<IBiometricAuthenticateService>().AuthenticateUserIDWithTouchID();
            if (result)
            {
                Proceed(true);
            }
            else
            {
                InfoText = authType == "TouchId" ? "Отпечаток не распознан. Попробуйте еще раз." : "Лицо не распознано. Попробуйте еще раз.";
            }
        }

        private async void Proceed(bool result)
        {
            MessagingCenter.Send<string>("AndroidAuth", "Cancel");
            Result = result;
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }

        public Command CancelCommand { get; }

        public BiometricTestPageVM()
        {
            CancelCommand = new Command(_ => Proceed(false));

            CheckBiometricTypes();
            StartBiometricCheck();
        }
    }
}
