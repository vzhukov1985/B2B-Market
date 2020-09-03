using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class PINSetPageVM:BaseVM
    {

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

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
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
            if (PINCode.Length == 4) { Proceed(); }
        }

        private async void Proceed()
        {
            await Application.Current.MainPage.Navigation.PopModalAsync();
        }

        public Command PINButtonTapCommand { get; }

        public PINSetPageVM(string title)
        {
            Title = title;

            PINButtonTapCommand = new Command<string>(s => AddPINNumber(s));
        }
    }
}
