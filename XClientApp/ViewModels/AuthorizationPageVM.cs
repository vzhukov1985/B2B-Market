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
using XClientApp.Services;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;

namespace XClientApp.ViewModels
{
    public class AuthorizationPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

		private readonly IDialogService DialogService;
		private readonly IPageService PageService;

		private string _login;
		public string Login
		{
			get { return _login; }
			set
			{
				_login = value;
				OnPropertyChanged("Login");
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

		public async void Authorize(string password)
		{
			using (MarketDbContext db = new MarketDbContext())
			{
				ClientUser user = await db.ClientsUsers.AsNoTracking().Where(o => o.Login == Login).FirstOrDefaultAsync();
				if ((user != null) && (Authentication.CheckPassword(password, user.PasswordHash)))
				{
					if (password == user.InitialPassword)
					{
						PageService.ShowFirstLoginPage(user);
					}
					else
					{
						PageService.ShowMainPage(user);
					}
					return;
				}
			}
			DialogService.ShowErrorDlg("Неверный логин или пароль");
		}

		public CommandType AuthorizeCommand { get; }

		public AuthorizationPageVM(IPageService pageService, IDialogService dialogService)
		{
			PageService = pageService;
			DialogService = dialogService;

			AuthorizeCommand = new CommandType();
			AuthorizeCommand.Create(o => { Authorize((string)o); });
		}
	}
}
