using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class ProductExtraProperty: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
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
                OnPropertyChanged("Product");
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

        private ProductExtraPropertyType _propertyType;
        public ProductExtraPropertyType PropertyType
        {
            get { return _propertyType; }
            set
            {
                _propertyType = value;
                OnPropertyChanged("PropertyType");
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
    }
}
