using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientApp_Mobile.Models
{
    public class OfferWithOrder : Offer
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
                OnPropertyChanged("OrderQuantityBeforeUserChanges");
            }
        }

        public bool IsQuantityWasChanged
        {
            get { return OrderQuantity != OrderQuantityBeforeUserChanges; }
        }
    }
}
