using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class ProductExtraProperty: INotifyPropertyChanged
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

        private Guid _propertyTypeId;
        public Guid PropertyTypeId
        {
            get { return _propertyTypeId; }
            set
            {
                _propertyTypeId = value;
                OnPropertyChanged("PropertyTypeId");
            }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
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

        private ProductExtraPropertyType _propertyType;
        public ProductExtraPropertyType PropertyType
        {
            get { return _propertyType; }
            set
            {
                _propertyType = value;
                if (_propertyType != null)
                    PropertyTypeId = _propertyType.Id;
                OnPropertyChanged("PropertyType");
            }
        }

        public static ProductExtraProperty CloneForDB(ProductExtraProperty source)
        {
            return new ProductExtraProperty { Id = source.Id, ProductId = source.ProductId, PropertyTypeId = source.PropertyTypeId, Value = source.Value };
        }
    }
}
