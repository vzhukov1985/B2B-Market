using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        private Guid? _volumeUnitId;
        public Guid? VolumeUnitId
        {
            get { return _volumeUnitId; }
            set
            {
                _volumeUnitId = value;
                OnPropertyChanged("VolumeUnitId");
            }
        }
    }
}
