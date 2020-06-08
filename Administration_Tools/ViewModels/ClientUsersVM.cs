﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Core.Services;
using Core.Models;
using Administration_Tools.Services;
using System.Diagnostics;

namespace Administration_Tools.ViewModels
{
    public class ClientUsersVM<CommandType>: INotifyPropertyChanged where CommandType: IRelayCommand, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

		private readonly IDialogService DialogService;

		private const int initialPasswordLength = 6;

		private Client _client;
		public Client Client
		{
			get { return _client; }
			set
			{
				_client = value;
				OnPropertyChanged("Client");
			}
		}

		private ObservableCollection<ClientUser> _users;
		public ObservableCollection<ClientUser> Users
		{
			get { return _users; }
			set
			{
				_users = value;
				OnPropertyChanged("Users");
			}
		}

		private ClientUser _selectedUser;
		public ClientUser SelectedUser
		{
			get { return _selectedUser; }
			set
			{
				_selectedUser = value;
				OnPropertyChanged("SelectedUser");
			}
		}


		internal void UpdateUsersList()
		{
			using (MarketDbContext db = new MarketDbContext())
			{
				Users = new ObservableCollection<ClientUser>(db.ClientsUsers.Where(c => c.ClientId == Client.Id).ToList());
			}
		}

		public void SaveUserChanges()
		{
			using (MarketDbContext db = new MarketDbContext())
			{
				if (SelectedUser != null)
				{
					if (Authentication.IsLoginAlreadyExists(SelectedUser.Login, SelectedUser.Id))
					{
						DialogService.ShowErrorDlg("Пользователь с таким логином уже существует. Измените логин");
						return;
					}

					db.ClientsUsers.Update(SelectedUser);
				}
				db.SaveChanges();
			}
		}

		public void AddUser()
		{
			ClientUser newUser = new ClientUser
			{
				Id = Guid.NewGuid(),
				ClientId = Client.Id,
				Name = "New",
				SurName = "User",
				IsAdmin = false,
				Login = Authentication.GenerateUniqueLogin(),
				InitialPassword = Authentication.GenerateRandomPassword(initialPasswordLength),
				IsPasswordWasReset = true,
			};
			newUser.PasswordHash = Authentication.HashPassword(newUser.InitialPassword);

			using (MarketDbContext db = new MarketDbContext())
			{
				db.ClientsUsers.Add(newUser);
				db.SaveChanges();
			}
			Users.Add(newUser);
			SelectedUser = newUser;
		}

		public void RemoveUser()
		{
			using (MarketDbContext db = new MarketDbContext())
			{
				db.ClientsUsers.Remove(SelectedUser);
				db.SaveChanges();
			}
			Users.Remove(SelectedUser);
		}

		public void ResetPassword()
		{
			using (MarketDbContext db = new MarketDbContext())
			{
				if (SelectedUser != null)
				{
					SelectedUser.InitialPassword = Authentication.GenerateRandomPassword(initialPasswordLength);
					SelectedUser.PasswordHash = Authentication.HashPassword(SelectedUser.InitialPassword);
					SelectedUser.IsPasswordWasReset = true;
					db.ClientsUsers.Update(SelectedUser);
				}
				db.SaveChanges();
			}
		}

		public CommandType UpdateUsersListCommand { get; }
		public CommandType AddUserCommand { get; }
		public CommandType RemoveUserCommand { get; }
		public CommandType SaveUserChangesCommand { get; }
		public CommandType ResetPasswordCommand { get; }

		public ClientUsersVM(Client SelectedClient, IDialogService dialogService)
		{
			
			Client = SelectedClient;
			DialogService = dialogService;

			UpdateUsersListCommand = new CommandType();
			UpdateUsersListCommand.Create(_ => { UpdateUsersList(); });
			AddUserCommand = new CommandType();
			AddUserCommand.Create(_ => { AddUser(); });
			RemoveUserCommand = new CommandType();
			RemoveUserCommand.Create(_ => { RemoveUser(); }, _ => SelectedUser != null);
			SaveUserChangesCommand = new CommandType();
			SaveUserChangesCommand.Create(_ => { SaveUserChanges(); }, _ => SelectedUser != null);
			ResetPasswordCommand = new CommandType();
			ResetPasswordCommand.Create(_ => { ResetPassword(); }, _ => SelectedUser != null);

			UpdateUsersListCommand.Execute(null);
		}
	}
}
