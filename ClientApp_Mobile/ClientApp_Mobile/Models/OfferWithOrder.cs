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

        private decimal _orderQuantity;
        public decimal OrderQuantity
        {
            get { return _orderQuantity; }
            set
            {
                _orderQuantity = value;
                OnPropertyChanged("OrderQuantity");
            }
        }

        private decimal _orderQuantityBeforeUserChanges;
        public decimal OrderQuantityBeforeUserChanges
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
