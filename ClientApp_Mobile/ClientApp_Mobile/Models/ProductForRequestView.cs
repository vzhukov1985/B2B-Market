using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ClientApp_Mobile.Models
{
    public enum ProductOrderAndRemainsState
    {
        Ok,
        OneSupplierNullRemains,
        AllSuppliersNullRemains,
        OneSupplierLessRemains,
        OneOfSuppliersLessRemains
    }

    public class ProductForRequestView : Product
    {
        private List<OfferWithOrder> _orders;
        public List<OfferWithOrder> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged("Orders");
            }
        }

        public ProductOrderAndRemainsState OrderAndRemainsState
        {
            get
            {
                if (Orders.All(o => o.Remains == 0 || o.Supplier.IsActive == false || o.IsActive == false))
                {
                    if (Orders.Count == 1)
                        return ProductOrderAndRemainsState.OneSupplierNullRemains;
                    else
                        return ProductOrderAndRemainsState.AllSuppliersNullRemains;
                }

                if (Orders.Any(o => o.OrderQuantity > o.Remains || o.Supplier.IsActive == false || o.IsActive == false))
                {
                    if (Orders.Count == 1)
                        return ProductOrderAndRemainsState.OneSupplierLessRemains;
                    else
                        return ProductOrderAndRemainsState.OneOfSuppliersLessRemains;
                }
                return ProductOrderAndRemainsState.Ok;
            }
        }

        public ProductForRequestView(Product product)
        {
            this.BestDiscountPriceOffer = product.BestDiscountPriceOffer;
            this.BestRetailPriceOffer = product.BestRetailPriceOffer;
            this.Category = product.Category;
            this.CategoryId = product.CategoryId;
            this.ExtraProperties = product.ExtraProperties;
            this.Favorites = product.Favorites;
            this.Id = product.Id;
            this.IsFavoriteForUser = product.IsFavoriteForUser;
            this.IsOfContractedSupplier = product.IsOfContractedSupplier;
            this.Name = product.Name;
            this.Offers = product.Offers;
            this.Volume = product.Volume;
            this.VolumeType = product.VolumeType;
            this.VolumeTypeId = product.VolumeTypeId;
            this.VolumeUnit = product.VolumeUnit;
            this.VolumeUnitId = product.VolumeUnitId;
        }
    }
}
