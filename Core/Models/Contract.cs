using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class Contract: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private int _clientId;
        public int ClientId
        {
            get { return _clientId; }
            set
            {
                _clientId = value;
                OnPropertyChanged("ClientId");
            }
        }

        private Client client;
        public Client Client
        {
            get { return client; }
            set
            {
                client = value;
                OnPropertyChanged("Client");
            }
        }

        private int _supplierId;
        public int SupplierId
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
                OnPropertyChanged("Supplier");
            }
        }


    }
}
