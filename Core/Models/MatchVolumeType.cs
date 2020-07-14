using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class MatchVolumeType:INotifyPropertyChanged
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


        private string _supplierVolumeTypeName;
        public string SupplierVolumeTypeName
        {
            get { return _supplierVolumeTypeName; }
            set
            {
                _supplierVolumeTypeName = value;
                OnPropertyChanged("SupplierVolumeTypeName");
            }
        }

        private Guid _volumeTypeId;
        public Guid VolumeTypeId
        {
            get { return _volumeTypeId; }
            set
            {
                _volumeTypeId = value;
                OnPropertyChanged("VolumeTypeId");
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
