using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class ArchivedRequestDetails
    {
        public List<ArchivedOrder> ArchivedOrders { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public string Comments { get; set; }
    }
}
