using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private Guid _productCategoryId;
        public Guid ProductCategoryId
        {
            get { return _productCategoryId; }
            set
            {
                _productCategoryId = value;
                OnPropertyChanged("ProductCategoryId");
            }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }


    }
}
