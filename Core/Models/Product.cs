using Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class Product: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private Guid _id;
        [Key]
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

        private ProductCategory _category;
        public ProductCategory Category
        {
            get { return _category; }
            set
            {
                _category = value;
                OnPropertyChanged("Category");
            }
        }

        private int _code;
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
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

        private VolumeType _volumeType;
        public VolumeType VolumeType
        {
            get { return _volumeType; }
            set
            {
                _volumeType = value;
                OnPropertyChanged("VolumeType");
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

        private VolumeUnit _volumeUnit;
        public VolumeUnit VolumeUnit
        {
            get { return _volumeUnit; }
            set
            {
                _volumeUnit = value;
                OnPropertyChanged("VolumeUnit");
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

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
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
        public byte[] Picture
        {
            get
            {
                return FTPManager.GetProductPicture(Id);
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

    }
}
