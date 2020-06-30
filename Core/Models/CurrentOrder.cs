using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.Models
{
    public class CurrentOrder: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private Guid _clientId;
        public Guid ClientId
        {
            get { return _clientId; }
            set
            {
                _clientId = value;
                OnPropertyChanged("ClientId");
            }
        }

        private Client _client;
        public Client Client
        {
            get { return _client; }
            set
            {
                _client = value;
                OnPropertyChanged("Client");
            }
        }

        private Guid _offerId;
        public Guid OfferId
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
                OnPropertyChanged("Offer");
            }
        }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

    }
}
