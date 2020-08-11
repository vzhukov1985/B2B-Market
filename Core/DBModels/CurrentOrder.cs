using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
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
                if (_client != null)
                    ClientId = _client.Id;
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
                if (_offer != null)
                    OfferId = _offer.Id;
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

        public static CurrentOrder CloneForDB(CurrentOrder order)
        {
            return new CurrentOrder
            {
                ClientId = order.ClientId,
                OfferId = order.OfferId,
                Quantity = order.Quantity
            };
        }

    }
}
