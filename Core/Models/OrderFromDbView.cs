using System;
namespace Core.Models
{
    public class OrderFromDbView
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public Uri ProductPictureUri { get; set; }
        public string ProductCategoryName { get; set; }
        public int ProductCode { get; set; }
        public string VolumeType { get; set; }
        public decimal Volume { get; set; }
        public string VolumeUnit { get; set; }
        public bool IsFavoriteForUser { get; set; }

        public Guid SupplierId { get; set; }
        public string SupplierShortName { get; set; }
        public string SupplierFullName { get; set; }
        public string SupplierBin { get; set; }
        public string SupplierCountry { get; set; }
        public string SupplierCity { get; set; }
        public string SupplierAddress { get; set; }
        public string SupplierPhone { get; set; }
        public string SupplierEmail { get; set; }
        public string SupplierFTPUser { get; set; }
        public string SupplierContactPersonName { get; set; }
        public string SupplierContactPersonPhone { get; set; }
        public bool IsSupplierActive { get; set; }

        public Guid ProductTopCategoryId { get; set; }
        public string ProductTopCategoryName { get; set; }

        public Guid OfferId { get; set; }
        public string SupplierProductCode { get; set; }
        public decimal Remains { get; set; }
        public bool IsActive { get; set; }
        public decimal PriceForClient { get; set; }
        public decimal OrderQuantity { get; set; }
        public string QuantityUnit { get; set; }

        public bool IsOfContractedSupplier { get; set; }
        public bool IsSelected { get; set; }
    }
}
