﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class ArchivedSupplier: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
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
        public string Bin
        {
            get { return _bin; }
            set
            {
                _bin = value;
                OnPropertyChanged("Bin");
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

        public string ContactPersonName { get; set; }

        public string ContactPersonPhone { get; set; }

        public static ArchivedSupplier CloneForDB(ArchivedSupplier archivedSupplier)
        {
            return new ArchivedSupplier
            {
                City = archivedSupplier.City,
                Country = archivedSupplier.Country,
                Address = archivedSupplier.Address,
                Bin = archivedSupplier.Bin,
                Email = archivedSupplier.Email,
                FullName = archivedSupplier.FullName,
                Id = archivedSupplier.Id,
                Phone = archivedSupplier.Phone,
                ShortName = archivedSupplier.ShortName
            };
        }
    }
}
