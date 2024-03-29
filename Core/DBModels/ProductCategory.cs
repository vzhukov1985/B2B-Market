﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class ProductCategory: INotifyPropertyChanged
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

        private Guid _midCategoryId;
        public Guid MidCategoryId
        {
            get { return _midCategoryId; }
            set
            {
                _midCategoryId = value;
                OnPropertyChanged("MidCategoryId");
            }
        }

        private MidCategory _midCategory;
        public MidCategory MidCategory
        {
            get { return _midCategory; }
            set
            {
                _midCategory = value;
                OnPropertyChanged("MidCategory");
            }
        }
    }
}
