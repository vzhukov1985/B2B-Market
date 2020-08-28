using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class Offer: INotifyPropertyChanged
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
                if (_supplier != null)
                    SupplierId = _supplier.Id;
                OnPropertyChanged("Supplier");
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

        private Product _product;
        public Product Product
        {
            get { return _product; }
            set
            {
                _product = value;
                if (_product != null)
                    ProductId = _product.Id;
                OnPropertyChanged("Product");
            }
        }

        private Guid _quantityUnitId;
        public Guid QuantityUnitId
        {
            get { return _quantityUnitId; }
            set
            {
                _quantityUnitId = value;
                OnPropertyChanged("QuantityUnitId");
            }
        }

        private QuantityUnit _quantityUnit;
        public QuantityUnit QuantityUnit
        {
            get { return _quantityUnit; }
            set
            {
                _quantityUnit = value;
                if (_quantityUnit != null)
                    QuantityUnitId = _quantityUnit.Id;
                OnPropertyChanged("QuantityUnit");
            }
        }

        private int _remains;
        public int Remains
        {
            get { return _remains; }
            set
            {
                _remains = value;
                OnPropertyChanged("Remains");
            }
        }

        private decimal _retailPrice;
        public decimal RetailPrice
        {
            get { return _retailPrice; }
            set
            {
                _retailPrice = value;
                OnPropertyChanged("RetailPrice");
            }
        }

        private decimal _discountPrice;
        public decimal DiscountPrice
        {
            get { return _discountPrice; }
            set
            {
                _discountPrice = value;
                OnPropertyChanged("DiscountPrice");
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        public static Offer CloneForDB(Offer offer)
        {
            return new Offer
            {
                Id = offer.Id,
                SupplierId = offer.SupplierId,
                SupplierProductCode = offer.SupplierProductCode,
                ProductId = offer.ProductId,
                QuantityUnitId = offer.QuantityUnitId,
                Remains = offer.Remains,
                RetailPrice = offer.RetailPrice,
                DiscountPrice = offer.DiscountPrice,
                IsActive = offer.IsActive
            };
        }

    }
}
