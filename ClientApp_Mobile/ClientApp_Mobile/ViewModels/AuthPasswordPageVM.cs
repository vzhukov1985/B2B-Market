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
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
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

        public async void Authorize()
        {
            IsBusy = true;
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    ClientUser user = db.ClientsUsers.Where(o => o.Login == Login).FirstOrDefault();
                    if ((user != null) && (Authentication.CheckPassword(Password, user.PasswordHash)))
                    {
                        UserService.CurrentUser = await db.ClientsUsers
                                                    .Where(u => u.Id == user.Id)
                                                    .Include(u => u.Favorites)
                                                    .ThenInclude(f => f.Product)
                                                    .Include(u => u.Client)
                                                    .ThenInclude(c => c.Contracts)
                                                    .ThenInclude(ct => ct.Supplier)
                                                    .Include(u => u.Client)
                                                    .ThenInclude(c => c.CurrentOrders)
                                                    .ThenInclude(o => o.Offer)
                                                    .ThenInclude(of => of.QuantityUnit)
                                                    .Include(u => u.Client)
                                                    .ThenInclude(c => c.CurrentOrders)
                                                    .ThenInclude(o => o.Offer)
                                                    .ThenInclude(of => of.Supplier)
                                                    .FirstOrDefaultAsync();
                        Device.BeginInvokeOnMainThread(() =>
                        {

                            if (Password == user.InitialPassword)
                            {
                                IsBusy = false;
                                //TODO:FirstLogin settings
                                Application.Current.MainPage = new AppShell();
                            }
                            else
                            {
                                IsBusy = false;
                                Application.Current.MainPage = new AppShell();
                            }
                        });
                        return;
                    }
                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    ShellDialogService.ShowErrorDlg("Неверный логин или пароль");
                    IsBusy = false;
                });
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    IsBusy = false;
                    ShellDialogService.ShowConnectionErrorDlg();
                });
                return;
            }
        }

        public Command AuthorizeCommand { get; }

        public AuthPasswordPageVM()
        {
            AuthorizeCommand = new Command(o => Authorize(), _ => !string.IsNullOrEmpty(Login));
        }
    }
}
