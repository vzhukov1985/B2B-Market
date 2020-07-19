using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class MatchQuantityUnit:INotifyPropertyChanged
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


        private string _supplierQUShortName;
        public string SupplierQUShortName
        {
            get { return _supplierQUShortName; }
            set
            {
                _supplierQUShortName = value;
                OnPropertyChanged("SupplierQUShortName");
            }
        }

        private string _supplierQUFullName;
        public string SupplierQUFullName
        {
            get { return _supplierQUFullName; }
            set
            {
                _supplierQUFullName = value;
                OnPropertyChanged("SupplierQUFullName");
            }
        }

        private Guid? _quantityUnitId;
        public Guid? QuantityUnitId
        {
            get { return _quantityUnitId; }
            set
            {
                _quantityUnitId = value;
                OnPropertyChanged("QuantityUnitId");
            }
        }
    }
}
