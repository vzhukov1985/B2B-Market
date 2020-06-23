using ClientApp.Services;
using Core.Models;
using Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClientApp.ViewModels
{
	public class DoublePassword
	{
		public string Password1 { get; set; }
		public string Password2 { get; set; }
	}

	public class FirstLoginPageVM<CommandType> : INotifyPropertyChanged where CommandType : IRelayCommand, new()
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

		private readonly IDialogService DialogService;
		private readonly IPageService PageService;

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

		public void SetNewPassword(DoublePassword newDoublePassword)
		{
			if (newDoublePassword.Password1 != newDoublePassword.Password2)
			{
				DialogService.ShowErrorDlg(ClientAppResourceManager.GetString("Error_PasswordsAreNotTheSame"));
				return;
			}

			User.PasswordHash = Authentication.HashPassword(newDoublePassword.Password1);
			using (MarketDbContext db = new MarketDbContext())
			{
				db.ClientsUsers.Update(User);
				db.SaveChanges();
			}
			PageService.ShowMainPage(User);
		}

		public CommandType SetNewPasswordCommand;

		public FirstLoginPageVM(ClientUser user, IDialogService dialogService, IPageService pageService)
		{
			User = user;
			DialogService = dialogService;
			PageService = pageService;

			SetNewPasswordCommand = new CommandType();
			SetNewPasswordCommand.Create(pwds => SetNewPassword((DoublePassword) pwds));
		}
	}
}
