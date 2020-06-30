using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class OrderByUser: Offer
    {
        private bool _isOfContractedSupplier;
        public bool IsOfContractedSupplier
        {
            get { return _isOfContractedSupplier; }
            set
            {
                _isOfContractedSupplier = value;
                OnPropertyChanged("IsOfContractedSupplier");
            }
        }

        private decimal _priceForClient;
        public decimal PriceForClient
        {
            get { return _priceForClient; }
            set
            {
                _priceForClient = value;
                OnPropertyChanged("PriceForClient");
            }
        }

        private int _orderQuantity;
        public int OrderQuantity
        {
            get { return _orderQuantity; }
            set
            {
                _orderQuantity = value;
                if (_orderQuantity < 0)
                    _orderQuantity = 0;
                if (_orderQuantity > Remains)
                    _orderQuantity = Remains;

                OnPropertyChanged("OrderQuantity");
            }
        }

        private int _orderQuantityBeforeUserChanges;
        public int OrderQuantityBeforeUserChanges
        {
            get { return _orderQuantityBeforeUserChanges; }
            set
            {
                _orderQuantityBeforeUserChanges = value;
                if (_orderQuantityBeforeUserChanges < 0)
                    _orderQuantityBeforeUserChanges = 0;
                if (_orderQuantityBeforeUserChanges > Remains)
                    _orderQuantityBeforeUserChanges = Remains;

                OnPropertyChanged("OrderQuantityBeforeUserChanges");
            }
        }

        public bool IsQuantityWasChanged
        {
            get { return OrderQuantity != OrderQuantityBeforeUserChanges; }
        }
    }
}
