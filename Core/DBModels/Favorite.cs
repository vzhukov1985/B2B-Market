using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core.DBModels
{
    public class Favorite:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private Guid _clientUserId;
        public Guid ClientUserId
        {
            get { return _clientUserId; }
            set
            {
                _clientUserId = value;
                OnPropertyChanged("ClientUserId");
            }
        }

        private ClientUser _clientUser;
        public ClientUser ClientUser
        {
            get { return _clientUser; }
            set
            {
                _clientUser = value;
                OnPropertyChanged("ClientUser");
            }
        }

        private Guid _productId;
        public Guid ProductId
        {
            get { return _productId; }
            set
            {
                _productId = value;
                OnPropertyChanged("ProductId");
            }
        }

        private Product _product;
        public Product Product
        {
            get { return _product; }
            set
            {
                _product = value;
                OnPropertyChanged("Product");
            }
        }
    }
}
