using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class ArchivedSupplier: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private Guid _id;
        [Key]
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
    }
}
