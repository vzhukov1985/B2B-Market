using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class MatchOffer:INotifyPropertyChanged
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

        private Guid _matchProductCategoryId;
        public Guid MatchProductCategoryId
        {
            get { return _matchProductCategoryId; }
            set
            {
                _matchProductCategoryId = value;
                OnPropertyChanged("MatchProductCategoryId");
            }
        }

        private MatchProductCategory _matchProductCategory;
        public MatchProductCategory MatchProductCategory
        {
            get { return _matchProductCategory; }
            set
            {
                _matchProductCategory = value;
                if (_matchProductCategory != null)
                    MatchProductCategoryId = _matchProductCategory.Id;
                OnPropertyChanged("MatchProductCategory");
            }
        }

        private Guid _matchVolumeTypeId;
        public Guid MatchVolumeTypeId
        {
            get { return _matchVolumeTypeId; }
            set
            {
                _matchVolumeTypeId = value;
                OnPropertyChanged("MatchVolumeTypeId");
            }
        }

        private MatchVolumeType _matchVolumeType;
        public MatchVolumeType MatchVolumeType
        {
            get { return _matchVolumeType; }
            set
            {
                _matchVolumeType = value;
                if (_matchVolumeType != null)
                    MatchVolumeTypeId = _matchVolumeType.Id;
                OnPropertyChanged("MatchVolumeType");
            }
        }

        private Guid _matchVolumeUnitId;
        public Guid MatchVolumeUnitId
        {
            get { return _matchVolumeUnitId; }
            set
            {
                _matchVolumeUnitId = value;
                OnPropertyChanged("MatchVolumeUnitId");
            }
        }

        private MatchVolumeUnit _matchVolumeUnit;
        public MatchVolumeUnit MatchVolumeUnit
        {
            get { return _matchVolumeUnit; }
            set
            {
                _matchVolumeUnit = value;
                if (_matchVolumeUnit != null)
                    MatchVolumeUnitId = _matchVolumeUnit.Id;
                OnPropertyChanged("MatchVolumeUnit");
            }
        }

        private decimal productVolume;
        public decimal ProductVolume
        {
            get { return productVolume; }
            set
            {
                productVolume = value;
                OnPropertyChanged("ProductVolume");
            }
        }

        private Guid _matchQuantityUnitId;
        public Guid MatchQuantityUnitId
        {
            get { return _matchQuantityUnitId; }
            set
            {
                _matchQuantityUnitId = value;
                OnPropertyChanged("MatchQuantityUnitId");
            }
        }

        private MatchQuantityUnit _matchQuantityUnit;
        public MatchQuantityUnit MatchQuantityUnit
        {
            get { return _matchQuantityUnit; }
            set
            {
                _matchQuantityUnit = value;
                if (_matchQuantityUnit != null)
                    MatchQuantityUnitId = _matchQuantityUnit.Id;
                OnPropertyChanged("MatchQuantityUnit");
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

        private Guid? _offerId;
        public Guid? OfferId
        {
            get { return _offerId; }
            set
            {
                _offerId = value;
                OnPropertyChanged("OfferId");
            }
        }

        private Offer _offer;
        public Offer Offer
        {
            get { return _offer; }
            set
            {
                _offer = value;
                if (_offer != null)
                    OfferId = _offer.Id;
                OnPropertyChanged("Offer");
            }
        }

        private ObservableCollection<MatchProductExtraProperty> _matchProductExtraProperties;
        public ObservableCollection<MatchProductExtraProperty> MatchProductExtraProperties
        {
            get { return _matchProductExtraProperties; }
            set
            {
                _matchProductExtraProperties = value;
                OnPropertyChanged("MatchProductExtraProperties");
            }
        }

        public static MatchOffer CloneForDB(MatchOffer matchOffer)
        {
            return new MatchOffer
            {
                Id = matchOffer.Id,
                SupplierId = matchOffer.SupplierId,
                SupplierProductCode = matchOffer.SupplierProductCode,
                ProductName = matchOffer.ProductName,
                MatchProductCategoryId = matchOffer.MatchProductCategoryId,
                MatchVolumeTypeId = matchOffer.MatchVolumeTypeId,
                MatchVolumeUnitId = matchOffer.MatchVolumeUnitId,
                ProductVolume = matchOffer.ProductVolume,
                MatchQuantityUnitId = matchOffer.MatchQuantityUnitId,
                Remains = matchOffer.Remains,
                RetailPrice = matchOffer.RetailPrice,
                DiscountPrice = matchOffer.DiscountPrice,
                OfferId = matchOffer.OfferId
            };
        }

        public MatchOffer()
        {
            MatchProductExtraProperties = new ObservableCollection<MatchProductExtraProperty>();
        }
    }
}
