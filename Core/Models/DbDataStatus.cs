using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class DbDataStatus
    {
        public bool IsConnected { get; set; }
        public int UnMatchedProductCategories { get; set; }
        public int UnMatchedVolumeTypes { get; set; }
        public int UnMatchedVolumeUnits { get; set; }
        public int UnMatchedQuantityUnits { get; set; }
        public int UnMatchedExtraProperties { get; set; }
        public int UnMatchedOffers { get; set; }

        public int ConflictedPics { get; set; }
        public int ProductsWithoutPics { get; set; }

        public int ConflictedDescs { get; set; }
        public int ProductsWithoutDescs { get; set; }

        public DbDataStatus()
        {
            IsConnected = false;
            UnMatchedProductCategories = 0;
            UnMatchedVolumeTypes = 0;
            UnMatchedVolumeUnits = 0;
            UnMatchedQuantityUnits = 0;
            UnMatchedExtraProperties = 0;
            UnMatchedOffers = 0;
            ConflictedPics = 0;
            ProductsWithoutPics = 0;
            ConflictedDescs = 0;
            ProductsWithoutDescs = 0;
        }
    }
}
