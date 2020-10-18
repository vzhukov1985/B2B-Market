using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Core.Models
{
    public class ProductWithOffersDbView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Uri PictureUri { get; set; }
        public bool IsOfContractedSupplier { get; set; }
        private bool _isFavoriteForUser;
        public bool IsFavoriteForUser
        {
            get { return _isFavoriteForUser; }
            set
            {
                _isFavoriteForUser = value;
                OnPropertyChanged("IsFavoriteForUser");
            }
        }
        public string CategoryName { get; set; }
        public string VolumeType { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; }
        public BestOffer BestRetailOffer { get; set; }
        public BestOffer BestDiscountOffer { get; set; }

        public class BestOffer
        {
            public decimal Price { get; set; }
            public string QuantityUnit { get; set; }
            public string SupplierName { get; set; }
        }
    }
}
