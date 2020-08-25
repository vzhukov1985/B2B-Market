using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class MatchProductExtraPropertyType: INotifyPropertyChanged
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


        private string _supplierProductExtraPropertyTypeName;
        public string SupplierProductExtraPropertyTypeName
        {
            get { return _supplierProductExtraPropertyTypeName; }
            set
            {
                _supplierProductExtraPropertyTypeName = value;
                OnPropertyChanged("SupplierProductExtraPropertyTypeName");
            }
        }

        private Guid? _productExtraPropertyTypeId;
        public Guid? ProductExtraPropertyTypeId
        {
            get { return _productExtraPropertyTypeId; }
            set
            {
                _productExtraPropertyTypeId = value;
                OnPropertyChanged("ProductExtraPropertyTypeId");
            }
        }

        private ProductExtraPropertyType _productExtraPropertyType;
        public ProductExtraPropertyType ProductExtraPropertyType
        {
            get { return _productExtraPropertyType; }
            set
            {
                _productExtraPropertyType = value;
                OnPropertyChanged("ProductExtraPropertyType");
            }
        }

    }
}
