using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class ClientUser : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

		private Guid _id;
		public Guid Id
		{
			get { return _id; }
			set
			{
				_id = value;
				OnPropertyChanged("Id");
			}
		}

		private Guid _clientId;
		public Guid ClientId
		{
			get { return _clientId; }
			set
			{
				_clientId = value;
				OnPropertyChanged("ClientId");
			}
		}

        private string _name;
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged("Name");
			}
		}

		private string _surname;
		public string Surname
		{
			get { return _surname; }
			set
			{
				_surname = value;
				OnPropertyChanged("Surname");
			}
		}

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

		private string _passwordHash;
		public string PasswordHash
		{
			get { return _passwordHash; }
			set
			{
				_passwordHash = value;
				OnPropertyChanged("PasswordHash");
			}
		}

		private bool _isAdmin;
		public bool IsAdmin
		{
			get { return _isAdmin; }
			set
			{
				_isAdmin = value;
				OnPropertyChanged("IsAdmin");
			}
		}

		private string _initialPassword;
		public string InitialPassword
		{
			get { return _initialPassword; }
			set
			{
				_initialPassword = value;
				OnPropertyChanged("InitialPassword");
			}
		}

        private string _pinHash;
        public string PinHash
        {
            get { return _pinHash; }
            set
            {
                _pinHash = value;
                OnPropertyChanged("PinHash");
            }
        }

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

		private List<Favorite> _favorites;
        public List<Favorite> Favorites
        {
            get { return _favorites; }
            set
            {
                _favorites = value;
                OnPropertyChanged("Favorites");
            }
        }

		[NotMapped]
		public bool UseBiometricAccess { get; set; }

        public ClientUser()
        {
			Favorites = new List<Favorite>();
        }

		public static ClientUser CloneForDb(ClientUser user)
        {
			return new ClientUser
			{
				Id = user.Id,
				ClientId = user.ClientId,
				Name = user.Name,
				Surname = user.Surname,
				Login = user.Login,
				PasswordHash = user.PasswordHash,
				IsAdmin = user.IsAdmin,
				InitialPassword = user.InitialPassword,
				PinHash = user.PinHash,
			};
        }
    }
}
