using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.ObjectModel;

namespace Core.DBModels
{

    public class Client : INotifyPropertyChanged
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

        private string _shortname;
        public string ShortName
        {
            get { return _shortname; }
            set
            {
                _shortname = value;
                OnPropertyChanged("ShortName");
            }
        }

        private string _fullname;
        public string FullName
        {
            get { return _fullname; }
            set
            {
                _fullname = value;
                OnPropertyChanged("FullName");
            }
        }

        private string _bin;
        public string BIN
        {
            get { return _bin; }
            set
            {
                _bin = value;
                OnPropertyChanged("BIN");
            }
        }

        private string _country;
        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                OnPropertyChanged("Country");
            }
        }

        private string _city;
        public string City
        {
            get { return _city; }
            set
            {
                _city = value;
                OnPropertyChanged("City");
            }
        }

        private string _address;
        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged("Address");
            }
        }

        private string _phone;
        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                OnPropertyChanged("Phone");
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged("Email");
            }
        }

        private string _contactPersonName;
        public string ContactPersonName
        {
            get { return _contactPersonName; }
            set
            {
                _contactPersonName = value;
                OnPropertyChanged("ContactPersonName");
            }
        }

        private string _contactPersonPhone;
        public string ContactPersonPhone
        {
            get { return _contactPersonPhone; }
            set
            {
                _contactPersonPhone = value;
                OnPropertyChanged("ContactPersonPhone");
            }
        }

        private List<ArchivedRequest> _archivedRequests;
        public List<ArchivedRequest> ArchivedRequests
        {
            get { return _archivedRequests; }
            set
            {
                _archivedRequests = value;
                OnPropertyChanged("ArchivedRequests");
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

        private ObservableCollection<Contract> _contracts;
        public ObservableCollection<Contract> Contracts
        {
            get { return _contracts; }
            set
            {
                _contracts = value;
                OnPropertyChanged("Contracts");
            }
        }
        private List<CurrentOrder> _currentOrders;
        public List<CurrentOrder> CurrentOrders
        {
            get { return _currentOrders; }
            set
            {
                _currentOrders = value;
                OnPropertyChanged("CurrentOrders");
            }
        }


        public Client()
        {
            Contracts = new ObservableCollection<Contract>();
            Users = new ObservableCollection<ClientUser>();
            CurrentOrders = new List<CurrentOrder>();
        }
    }
}

