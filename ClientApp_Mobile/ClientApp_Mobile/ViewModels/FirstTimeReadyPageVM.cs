using ClientApp_Mobile.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class FirstTimeReadyPageVM:BaseVM
    {
        private void Proceed()
        {
            IsBusy = true;
            AppPageService.GoToMainMage();
            IsBusy = false;
        }

        public Command ProceedCommand { get; }

        public FirstTimeReadyPageVM()
        {
            ProceedCommand = new Command(_ => Proceed());
        }
    }
}
