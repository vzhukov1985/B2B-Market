using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class MatchVolumeType:INotifyPropertyChanged
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

        private Guid? _volumeTypeId;
        public Guid? VolumeTypeId
        {
            get { return _volumeTypeId; }
            set
            {
                _volumeTypeId = value;
                OnPropertyChanged("VolumeTypeId");
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

        private VolumeType _volumeType;
        public VolumeType VolumeType
        {
            get { return _volumeType; }
            set
            {
                _volumeType = value;
                OnPropertyChanged("VolumeType");
            }
        }
    }
}
