using ClientApp_Mobile.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class BaseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public CancellationTokenSource CTS { get; set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                if (_isBusy)
                    CTS = new CancellationTokenSource();
                else
                    CTS.Dispose();
                OnPropertyChanged("IsBusy");
            }
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public Command ContactSupportCommand => new Command(async() =>
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = $"Проблема c моб. прил. B2B Market HoReCa ({UserService.CurrentUser?.Login})",
                    To = new List<string> { "support@b2bmarket.kz" }
                };
                await Email.ComposeAsync(message);
            }
            catch
            {
                return;
            }
        });

        public BaseVM()
        {

        }

    }
}
