﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class MatchProductCategory: INotifyPropertyChanged
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


        private Guid _supplierId;
        public Guid SupplierId
        {
            get { return _supplierId; }
            set
            {
                _supplierId = value;
                OnPropertyChanged("SupplierId");
            }
        }

        private Supplier _supplier;
        public Supplier Supplier
        {
            get { return _supplier; }
            set
            {
                _supplier = value;
                OnPropertyChanged("Supplier");
            }
        }


        private string _supplierCategoryName;
        public string SupplierCategoryName
        {
            get { return _supplierCategoryName; }
            set
            {
                _supplierCategoryName = value;
                OnPropertyChanged("SupplierCategoryName");
            }
        }

        private Guid? _productCategoryId;
        public Guid? ProductCategoryId
        {
            get { return _productCategoryId; }
            set
            {
                _productCategoryId = value;
                OnPropertyChanged("ProductCategoryId");
            }
        }

    }
}
