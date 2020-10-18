using System;
using System.Collections.Generic;
using Core.DBModels;

namespace Core.Models
{
    public class RequestForConfirmation : ArchivedRequest
    {
        public string FTPSupplierFolder { get; set; }

        public DateTime DeliveryDate
        {
            get { return DeliveryDateTime.Date; }
            set
            {
                DeliveryDateTime = new DateTime(value.Year, value.Month, value.Day, DeliveryDateTime.Hour, DeliveryDateTime.Minute, DeliveryDateTime.Second);
                OnPropertyChanged("DeliveryDate");
            }
        }

        public TimeSpan DeliveryTime
        {
            get { return DeliveryDateTime.TimeOfDay; }
            set
            {
                DeliveryDateTime = new DateTime(DeliveryDateTime.Year, DeliveryDateTime.Month, DeliveryDateTime.Day, value.Hours, value.Minutes, value.Seconds);
                OnPropertyChanged("DeliveryTime");
            }
        }

        public List<OrderForConfirmation> OrdersToConfirm { get; set; }
    }

    public class OrderForConfirmation : ArchivedOrder
    {
        public Guid OfferId { get; set; }
        public decimal Remains { get; set; }
        public ProductForConfirmation Product { get; set; }
    }

    public class ProductForConfirmation
    {
        public string Name { get; set; }
        public Uri PictureUri { get; set; }
        public string CategoryName { get; set; }
        public string VolumeType { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; }
    }
}
