using ClientApp_Mobile.Services;
using Core.DBModels;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class MainPageVM:BaseVM
    {
        private CurrentUserInfo _user;
        public CurrentUserInfo User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        public Command ChangeUserCommand { get; }

        public MainPageVM()
        {
            User = AppSettings.CurrentUser;
            Task.Run(async () => AppSettings.ArchivedOrderStatusTypes = await ApiConnect.GetArchivedOrderStatuses());

            ChangeUserCommand = new Command(_ => AppPageService.GoToAuthorizationPage());
        }

    }
}
