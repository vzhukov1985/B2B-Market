using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Core.Resources;

namespace Core.DBModels
{
    public class ArchivedRequest: INotifyPropertyChanged
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

        private string _senderName;
        public string SenderName
        {
            get { return _senderName; }
            set
            {
                _senderName = value;
                OnPropertyChanged("SenderName");
            }
        }

        private string _senderSurname;
        public string SenderSurname
        {
            get { return _senderSurname; }
            set
            {
                _senderSurname = value;
                OnPropertyChanged("SenderSurname");
            }
        }

        private int _itemsQuantity;
        public int ItemsQuantity
        {
            get { return _itemsQuantity; }
            set
            {
                _itemsQuantity = value;
                OnPropertyChanged("ItemsQuantity");
            }
        }

        private int _productsQuantity;
        public int ProductsQuantity
        {
            get { return _productsQuantity; }
            set
            {
                _productsQuantity = value;
                OnPropertyChanged("ProductsQuantity");
            }
        }

        private Guid _archivedSupplierId;
        public Guid ArchivedSupplierId
        {
            get { return _archivedSupplierId; }
            set
            {
                _archivedSupplierId = value;
                OnPropertyChanged("ArchivedSupplierId");
            }
        }

        private ArchivedSupplier _archivedSupplier;
        public ArchivedSupplier ArchivedSupplier
        {
            get { return _archivedSupplier; }
            set
            {
                _archivedSupplier = value;
                if (_archivedSupplier != null)
                    ArchivedSupplierId = _archivedSupplier.Id;
                OnPropertyChanged("ArchivedSupplier");
            }
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get { return _totalPrice; }
            set
            {
                _totalPrice = value;
                OnPropertyChanged("TotalPrice");
            }
        }

        private DateTime _dateTimeSent;
        public DateTime DateTimeSent
        {
            get { return _dateTimeSent; }
            set
            {
                _dateTimeSent = value;
                OnPropertyChanged("DateTimeSent");
            }
        }

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
        [NotMapped]
        public DateTime DeliveryDate
        {
            get { return DeliveryDateTime.Date; }
            set
            {
                DeliveryDateTime = new DateTime(value.Year, value.Month, value.Day, DeliveryDateTime.Hour, DeliveryDateTime.Minute, DeliveryDateTime.Second);
                OnPropertyChanged("DeliveryDate");
            }
        }

        [NotMapped]
        public TimeSpan DeliveryTime
        {
            get { return DeliveryDateTime.TimeOfDay; }
            set
            {
                DeliveryDateTime = new DateTime(DeliveryDateTime.Year, DeliveryDateTime.Month, DeliveryDateTime.Day, value.Hours, value.Minutes, value.Seconds);
                OnPropertyChanged("DeliveryTime");
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

        private ObservableCollection<ArchivedOrder> _archivedOrders;
        public ObservableCollection<ArchivedOrder> ArchivedOrders
        {
            get { return _archivedOrders; }
            set
            {
                _archivedOrders = value;
                OnPropertyChanged("ArchivedOrders");
            }
        }

        private ObservableCollection<ArchivedRequestsStatus> _archivedRequestsStatuses;
        public ObservableCollection<ArchivedRequestsStatus> ArchivedRequestsStatuses
        {
            get { return _archivedRequestsStatuses; }
            set
            {
                _archivedRequestsStatuses = value;
                OnPropertyChanged("ArchivedRequestsStatuses");
            }
        }

        [NotMapped]
        public string StatusName
        {
            get
            {
                return ArchivedRequestsStatuses == null ? "NONE" : ArchivedRequestsStatuses.OrderBy(st => st.DateTime).Last().ArchivedRequestStatusType.Name;
            }
        }

        [NotMapped]
        public string StatusDescription
        {
            get
            {
                    return ArchivedRequestsStatuses == null ? "Нет данных" : ArchivedRequestsStatuses.OrderBy(st => st.DateTime).Last().ArchivedRequestStatusType.Description;
            }
        }

        [NotMapped]
        public byte[] StatusPicture
        {
            get
            {
                if (ArchivedRequestsStatuses != null)
                {
                    switch (ArchivedRequestsStatuses.OrderBy(st => st.DateTime).Last().ArchivedRequestStatusType.Name)
                    {
                        case "PENDING":
                            return CoreResources.Request_Pending;
                        case "REJECTED":
                            return CoreResources.Request_Rejected;
                        case "ACCEPTED":
                            return CoreResources.Request_Accepted;
                        default:
                            return null;
                    }
                }
                return null;
            }
        }

        public ArchivedRequest()
        {
            ArchivedOrders = new ObservableCollection<ArchivedOrder>();
            ArchivedRequestsStatuses = new ObservableCollection<ArchivedRequestsStatus>();
        }

        public static ArchivedRequest CloneForDb(ArchivedRequest request)
        {
            return new ArchivedRequest
            {
                Id = request.Id,
                Code = request.Code,
                ClientId = request.ClientId,
                SenderName = request.SenderName,
                SenderSurname = request.SenderSurname,
                ItemsQuantity = request.ItemsQuantity,
                ProductsQuantity = request.ProductsQuantity,
                ArchivedSupplierId = request.ArchivedSupplierId,
                TotalPrice = request.TotalPrice,
                DateTimeSent = request.DateTimeSent,
                DeliveryDateTime = request.DeliveryDateTime,
                Comments = request.Comments
            };
        }
    }
}
