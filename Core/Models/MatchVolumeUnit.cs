using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class MatchVolumeUnit: INotifyPropertyChanged
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

        private string _supplierVUShortName;
        public string SupplierVUShortName
        {
            get { return _supplierVUShortName; }
            set
            {
                _supplierVUShortName = value;
                OnPropertyChanged("SupplierVUShortName");
            }
        }

        private string _supplierVUFullName;
        public string SupplierVUFullName
        {
            get { return _supplierVUFullName; }
            set
            {
                _supplierVUFullName = value;
                OnPropertyChanged("SupplierVUFullName");
            }
        }

        private Guid _volumeUnitId;
        public Guid VolumeUnitId
        {
            get { return _volumeUnitId; }
            set
            {
                _volumeUnitId = value;
                OnPropertyChanged("VolumeUnitId");
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
