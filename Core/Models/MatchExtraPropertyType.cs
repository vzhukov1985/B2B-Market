using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class MatchExtraPropertyType: INotifyPropertyChanged
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

        private string _supplierEPTypeName;
        public string SupplierEPTypeName
        {
            get { return _supplierEPTypeName; }
            set
            {
                _supplierEPTypeName = value;
                OnPropertyChanged("SupplierEPTypeName");
            }
        }

        private Guid _extraPropertyTypeId;
        public Guid ExtraPropertyTypeId
        {
            get { return _extraPropertyTypeId; }
            set
            {
                _extraPropertyTypeId = value;
                OnPropertyChanged("ExtraPropertyTypeId");
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
