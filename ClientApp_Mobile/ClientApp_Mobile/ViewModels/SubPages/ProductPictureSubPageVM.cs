using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ClientApp_Mobile.ViewModels.SubPages
{
    [QueryProperty("ProductId","id")]
    [QueryProperty("ProductName","name")]
    class ProductPictureSubPageVM: BaseVM
    {
        private string _productName;
        public string ProductName
        {
            get { return _productName; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _productName = Uri.UnescapeDataString(value);
                    OnPropertyChanged("ProductName");
                }
            }
        }

        private string _productId;
        public string ProductId
        {
            get { return _productId; }
            set
            {
                _productId = value;
                Product = string.IsNullOrEmpty(value) ? null : new Product() { Id = new Guid(Uri.UnescapeDataString(value)) };
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

        public ProductPictureSubPageVM()
        {

        }
    }
}
