using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class MatchProduct: INotifyPropertyChanged
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

        private string _supplierProductCode;
        public string SupplierProductCode
        {
            get { return _supplierProductCode; }
            set
            {
                _supplierProductCode = value;
                OnPropertyChanged("SupplierProductCode");
            }
        }

        private Guid _productId;
        public Guid ProductId
        {
            get { return _productId; }
            set
            {
                _productId = value;
                OnPropertyChanged("ProductId");
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
