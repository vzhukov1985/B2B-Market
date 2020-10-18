using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.DBModels;

namespace Core.Models
{
    public class ArchivedRequestForClientDbView:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public Guid Id { get; set; }
        public int Code { get; set; }
        public string StatusName { get; set; }
        public string StatusDescription { get; set; }
        public DateTime DateTimeSent { get; set; }
        public string ArchivedSupplierName { get; set; }
        public string SenderName { get; set; }
        public string SenderSurname { get; set; }
        public int ItemsQuantity { get; set; }
        public int ProductsQuantity { get; set; }
        public decimal TotalPrice { get; set; }

        private DateTime _deliveryDateTime;
        public DateTime DeliveryDateTime
        {
            get { return _deliveryDateTime; }
            set
            {
                _deliveryDateTime = value;
                OnPropertyChanged("DeliveryDateTime");
            }
        }

        private string _comments;
        public string Comments
        {
            get { return _comments; }
            set
            {
                _comments = value;
                OnPropertyChanged("Comments");
            }
        }

        public List<ArchivedOrder> ArchivedOrders { get; set; }
        private ObservableCollection<ArchivedRequestsStatus> _archivedRequestStatuses;
        
        public ObservableCollection<ArchivedRequestsStatus> ArchivedRequestsStatuses
        {
            get { return _archivedRequestStatuses; }
            set
            {
                _archivedRequestStatuses = value;
                OnPropertyChanged("ArchivedRequestsStatuses");
            }
        }
    }
}
