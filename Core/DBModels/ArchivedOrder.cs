using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class ArchivedOrder: INotifyPropertyChanged
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

        private Guid _archivedRequestId;
        public Guid ArchivedRequestId
        {
            get { return _archivedRequestId; }
            set
            {
                _archivedRequestId = value;
                OnPropertyChanged("ArchivedRequestId");
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

        private string _productName;
        public string ProductName
        {
            get { return _productName; }
            set
            {
                _productName = value;
                OnPropertyChanged("ProductName");
            }
        }

        private string _productCategory;
        public string ProductCategory
        {
            get { return _productCategory; }
            set
            {
                _productCategory = value;
                OnPropertyChanged("ProductCategory");
            }
        }

        private int _productCode;
        public int ProductCode
        {
            get { return _productCode; }
            set
            {
                _productCode = value;
                OnPropertyChanged("ProductCode");
            }
        }

        private string _productVolumeType;
        public string ProductVolumeType
        {
            get { return _productVolumeType; }
            set
            {
                _productVolumeType = value;
                OnPropertyChanged("ProductVolumeType");
            }
        }

        private string _productVolumeUnit;
        public string ProductVolumeUnit
        {
            get { return _productVolumeUnit; }
            set
            {
                _productVolumeUnit = value;
                OnPropertyChanged("ProductVolumeUnit");
            }
        }

        private decimal _productVolume;
        public decimal ProductVolume
        {
            get { return _productVolume; }
            set
            {
                _productVolume = value;
                OnPropertyChanged("ProductVolume");
            }
        }


        private string _quantityUnit;
        public string QuantityUnit
        {
            get { return _quantityUnit; }
            set
            {
                _quantityUnit = value;
                OnPropertyChanged("QuantityUnit");
            }
        }


        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        private decimal _price;
        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged("Price");
            }
        }

        private ArchivedRequest _archivedRequest;
        public ArchivedRequest ArchivedRequest
        {
            get { return _archivedRequest; }
            set
            {
                _archivedRequest = value;
                if (_archivedRequest != null)
                    ArchivedRequestId = ArchivedRequest.Id;
                OnPropertyChanged("ArchivedRequest");
            }
        }

        private Guid _offerId;
        [NotMapped]
        public Guid OfferId
        {
            get { return _offerId; }
            set
            {
                _offerId = value;
                OnPropertyChanged("OfferId");
            }
        }

        private Guid _productId;
        [NotMapped]
        public Guid ProductId
        {
            get { return _productId; }
            set
            {
                _productId = value;
                OnPropertyChanged("ProductId");
            }
        }

        private Product _product;
        [NotMapped]
        public Product Product
        {
            get { return _product; }
            set
            {
                _product = value;
                OnPropertyChanged("Product");
            }
        }

        private int _remains;
        [NotMapped]
        public int Remains
        {
            get { return _remains; }
            set
            {
                _remains = value;
                OnPropertyChanged("Remains");
            }
        }


        public static ArchivedOrder CloneForDB(ArchivedOrder order)
        {
            return new ArchivedOrder
            {
                Id = order.Id,
                ArchivedRequestId = order.ArchivedRequestId,
                SupplierProductCode = order.SupplierProductCode,
                ProductName = order.ProductName,
                ProductCategory = order.ProductCategory,
                ProductCode = order.ProductCode,
                ProductVolumeType = order.ProductVolumeType,
                ProductVolumeUnit = order.ProductVolumeUnit,
                ProductVolume = order.ProductVolume,
                QuantityUnit = order.QuantityUnit,
                Quantity = order.Quantity,
                Price = order.Price
            };
        }

    }
}
