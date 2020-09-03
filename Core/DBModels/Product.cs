using Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.DBModels
{
    public class Product: INotifyPropertyChanged
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

        private Guid _categoryId;
        public Guid CategoryId
        {
            get { return _categoryId; }
            set
            {
                _categoryId = value;
                OnPropertyChanged("CategoryId");
            }
        }

        private int _code;
        public int Code
        {
            get { return _code; }
            set
            {
                _code = value;
                OnPropertyChanged("Code");
            }
        }


        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private Guid _volumeTypeId;
        public Guid VolumeTypeId
        {
            get { return _volumeTypeId; }
            set
            {
                _volumeTypeId = value;
                OnPropertyChanged("VolumeTypeId");
            }
        }

        private Guid _volumeUnitId;
        public Guid VolumeUnitId
        {
            get { return _volumeUnitId; }
            set
            {
                _volumeUnitId = value;
                OnPropertyChanged("VolumeUnitId");
            }
        }

        private decimal _Volume;
        public decimal Volume
        {
            get { return _Volume; }
            set
            {
                _Volume = value;
                OnPropertyChanged("Volume");
            }
        }

        private ProductCategory _category;
        public ProductCategory Category
        {
            get { return _category; }
            set
            {
                _category = value;
                if (_category != null)
                    CategoryId = _category.Id;
                OnPropertyChanged("Category");
            }
        }

        private VolumeType _volumeType;
        public VolumeType VolumeType
        {
            get { return _volumeType; }
            set
            {
                _volumeType = value;
                if (_volumeType != null)
                    VolumeTypeId = _volumeType.Id;
                OnPropertyChanged("VolumeType");
            }
        }

        private VolumeUnit _volumeUnit;
        public VolumeUnit VolumeUnit
        {
            get { return _volumeUnit; }
            set
            {
                _volumeUnit = value;
                if (_volumeUnit != null)
                    VolumeUnitId = _volumeUnit.Id;
                OnPropertyChanged("VolumeUnit");
            }
        }

        private ProductDescription _description;
        public ProductDescription Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        private ObservableCollection<ProductExtraProperty> _extraProperties;
        public ObservableCollection<ProductExtraProperty> ExtraProperties
        {
            get { return _extraProperties; }
            set
            {
                _extraProperties = value;
                OnPropertyChanged("ExtraProperties");
            }
        }



        private ObservableCollection<Favorite> _favorites;
        public ObservableCollection<Favorite> Favorites
        {
            get { return _favorites; }
            set
            {
                _favorites = value;
                OnPropertyChanged("Favorites");
            }
        }

        private ObservableCollection<Offer> _offers;
        public ObservableCollection<Offer> Offers
        {
            get { return _offers; }
            set
            {
                _offers = value;
                OnPropertyChanged("Offers");
            }
        }

        private Offer _bestRetailPriceOffer;
        [NotMapped]
        public Offer BestRetailPriceOffer
        {
            get { return _bestRetailPriceOffer; }
            set
            {
                _bestRetailPriceOffer = value;
                OnPropertyChanged("BestRetailPriceOffer");
            }
        }

        private Offer _bestDiscountPriceOffer;
        [NotMapped]
        public Offer BestDiscountPriceOffer
        {
            get { return _bestDiscountPriceOffer; }
            set
            {
                _bestDiscountPriceOffer = value;
                OnPropertyChanged("BestDiscountPriceOffer");
            }
        }


        private bool _isFavoriteForUser;
        [NotMapped]
        public bool IsFavoriteForUser
        {
            get { return _isFavoriteForUser; }
            set
            {
                _isFavoriteForUser = value;
                OnPropertyChanged("IsFavoriteForUser");
            }
        }

        [NotMapped]
        public Uri PictureUri
        {
            get
            {
                return HTTPManager.GetMatchedProductPictureUri(Id);
            }
        }


        private bool _isOfContractedSupplier;
        [NotMapped]
        public bool IsOfContractedSupplier
        {
            get { return _isOfContractedSupplier; }
            set
            {
                _isOfContractedSupplier = value;
                OnPropertyChanged("IsOfContractedSupplier");
            }
        }

        public static Product CloneForDB(Product product)
        {
            return new Product
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Code = product.Code,
                Name = product.Name,
                VolumeTypeId = product.VolumeTypeId,
                VolumeUnitId = product.VolumeUnitId,
                Volume = product.Volume
            };
        }

        public Product()
        {
            Offers = new ObservableCollection<Offer>();
            Favorites = new ObservableCollection<Favorite>();
            ExtraProperties = new ObservableCollection<ProductExtraProperty>();
        }
    }
}
