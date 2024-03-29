﻿using System;
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
    public class ArchivedRequest : INotifyPropertyChanged
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

        private Guid? _clientId;
        public Guid? ClientId
        {
            get { return _clientId; }
            set
            {
                _clientId = value;
                OnPropertyChanged("ClientId");
            }
        }

        public Guid ArchivedClientId { get; set; }

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

        public Guid? SupplierId { get; set; }

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

        private ArchivedClient _archivedClient;
        public ArchivedClient ArchivedClient
        {
            get { return _archivedClient; }
            set
            {
                _archivedClient = value;
                OnPropertyChanged("ArchivedClient");
            }
        }



        private List<ArchivedOrder> _archivedOrders;
        public List<ArchivedOrder> ArchivedOrders
        {
            get { return _archivedOrders; }
            set
            {
                _archivedOrders = value;
                OnPropertyChanged("ArchivedOrders");
            }
        }

        private List<ArchivedRequestsStatus> _archivedRequestsStatuses;
        public List<ArchivedRequestsStatus> ArchivedRequestsStatuses
        {
            get { return _archivedRequestsStatuses; }
            set
            {
                _archivedRequestsStatuses = value;
                OnPropertyChanged("ArchivedRequestsStatuses");
            }
        }
        //TODO: Remove when evrything is done
/*        [NotMapped]
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
        }*/

        public ArchivedRequest()
        {
            ArchivedOrders = new List<ArchivedOrder>();
            ArchivedRequestsStatuses = new List<ArchivedRequestsStatus>();
        }

        public static ArchivedRequest CloneForDb(ArchivedRequest request)
        {
            return new ArchivedRequest
            {
                Id = request.Id,
                Code = request.Code,
                ClientId = request.ClientId,
                ArchivedClientId = request.ArchivedClientId,
                SenderName = request.SenderName,
                SenderSurname = request.SenderSurname,
                ItemsQuantity = request.ItemsQuantity,
                ProductsQuantity = request.ProductsQuantity,
                SupplierId = request.SupplierId,
                ArchivedSupplierId = request.ArchivedSupplierId,
                TotalPrice = request.TotalPrice,
                DateTimeSent = request.DateTimeSent,
                DeliveryDateTime = request.DeliveryDateTime,
                Comments = request.Comments
            };
        }
    }
}
