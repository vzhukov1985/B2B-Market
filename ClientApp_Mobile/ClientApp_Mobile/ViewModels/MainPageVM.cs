using ClientApp_Mobile.Services;
using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels
{
    public class MainPageVM:BaseVM
    {
        private ClientUser _user;
        public ClientUser User
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
            User = UserService.CurrentUser;

            ChangeUserCommand = new Command(_ => AppPageService.GoToAuthorizationPage());
        }

    }
}
