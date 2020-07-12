using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Core.Services
{
    public class XMLProcessor
    {
        public static void ExtractAllProductsToXML(string fileName, Guid supplierId)
        {
            List<Offer> offers;
            Supplier supplier;
            using (MarketDbContext db = new MarketDbContext())
            {
                offers = db.Offers
                    .Where(of => of.SupplierId == supplierId)
                    .Include(of => of.Product)
                    .ThenInclude(p => p.Category)
                    .Include(of => of.Product)
                    .ThenInclude(p => p.ExtraProperties)
                    .ThenInclude(pr => pr.PropertyType)
                    .Include(of => of.Product)
                    .ThenInclude(p => p.VolumeType)
                    .Include(of => of.Product)
                    .ThenInclude(p => p.VolumeUnit)
                    .Include(of => of.QuantityUnit)
                    .ToList();

                supplier = db.Suppliers
                    .Where(s => s.Id == supplierId)
                    .FirstOrDefault();
            }


            XDocument xDoc = new XDocument();

            XElement xExtraction = new XElement("OffersExtraction");
            xExtraction.Add(new XAttribute("Version", "1.0"));

            XElement xSupplierInfo = new XElement("SupplierInfo");
            xSupplierInfo.Add(new XAttribute("Id", supplier.Id.ToString()));
            xSupplierInfo.Add(new XElement("ShortName", supplier.ShortName));
            xSupplierInfo.Add(new XElement("FullName", supplier.FullName));
            xSupplierInfo.Add(new XElement("BIN", supplier.BIN));
            xSupplierInfo.Add(new XElement("Address", supplier.Address));
            xSupplierInfo.Add(new XElement("Phone", supplier.Phone));
            xSupplierInfo.Add(new XElement("EMail", supplier.Email));


            XElement xOffers = new XElement("Offers");
            foreach (Offer offer in offers)
            {
                XElement xOffer = new XElement("Offer");
                XAttribute xSupplierProductCode = new XAttribute("SupplierProductCode", offer.SupplierProductCode);
                
                XElement xProduct = new XElement("Product");
                xProduct.Add(new XElement("Name", offer.Product.Name));
                xProduct.Add(new XElement("Category", offer.Product.Category.Name));
                xProduct.Add(new XElement("VolumeType", offer.Product.VolumeType.Name));

                XElement xVolumeUnit = new XElement("VolumeUnit");
                xVolumeUnit.Add(new XAttribute("ShortName", offer.Product.VolumeUnit.ShortName));
                xVolumeUnit.Add(new XAttribute("FullName", offer.Product.VolumeUnit.FullName));
                xProduct.Add(xVolumeUnit);
                xProduct.Add(new XElement("Volume", offer.Product.Volume));

                XElement xProductExtraProperties = new XElement("ExtraProperties");
                foreach (ProductExtraProperty productExtraProperty in offer.Product.ExtraProperties)
                {
                    XElement xProductExtraProperty = new XElement("ExtraProperty");

                    xProductExtraProperty.Add(new XAttribute("Type", productExtraProperty.PropertyType));
                    xProductExtraProperty.Add(new XAttribute("Value", productExtraProperty.Value));
                    xProductExtraProperties.Add(xProductExtraProperty);
                }
                xProduct.Add(xProductExtraProperties);

                XElement xQuantityUnit = new XElement("QuantityUnit");
                xQuantityUnit.Add(new XAttribute("ShortName", offer.QuantityUnit.ShortName));
                xQuantityUnit.Add(new XAttribute("FullName", offer.QuantityUnit.FullName));

                XElement xRemains = new XElement("Remains", offer.Remains);

                XElement xRetailPrice = new XElement("RetailPrice", offer.RetailPrice);

                XElement xDiscountPrice = new XElement("DiscountPrice", offer.DiscountPrice);

                xOffer.Add(xSupplierProductCode);

                xOffer.Add(xProduct);
                xOffer.Add(xQuantityUnit);
                xOffer.Add(xRemains);
                xOffer.Add(xRetailPrice);
                xOffer.Add(xDiscountPrice);

                xOffers.Add(xOffer);
            }
            xExtraction.Add(xSupplierInfo);
            xExtraction.Add(xOffers);
            xDoc.Add(xExtraction);
            xDoc.Save(fileName);
        }

        public static void RequestProductsDescription(List<Offer> offers, Guid supplierId)
        {
            Supplier supplier;
            using (MarketDbContext db = new MarketDbContext())
            {
                supplier = db.Suppliers
                    .Where(s => s.Id == supplierId)
                    .FirstOrDefault();
            }
            XDocument xDoc = new XDocument();

            XElement xRequests = new XElement("RequestProductsDescriptions");
            xRequests.Add(new XAttribute("Version", "1.0"));

            foreach (Offer offer in offers)
            {

                XElement xRequest = new XElement("Request");
                xRequest.Add(new XAttribute("SupplierProductCode", offer.SupplierProductCode));
                xRequests.Add(xRequest);
            }

            xDoc.Add(xRequests);
            xDoc.Save("d://requestProductInfo.xml");
        }

        public static void ProductsDescriptions(List <Offer> offers, Guid supplierId)
        {
            Supplier supplier;
            using (MarketDbContext db = new MarketDbContext())
            {
                supplier = db.Suppliers
                    .Where(s => s.Id == supplierId)
                    .FirstOrDefault();
            }
            XDocument xDoc = new XDocument();

            XElement xDescriptions = new XElement("ProductsDescriptions");
            xDescriptions.Add(new XAttribute("Version", "1.0"));

            foreach (Offer offer in offers)
            {

                XElement xProductDesc = new XElement("ProductDescription");
                xProductDesc.Add(new XAttribute("SupplierProductCode", offer.SupplierProductCode));
                xProductDesc.Add(new XElement("PictureFile", "Pics\\" + offer.Product.Id.ToString() + ".png"));
                xProductDesc.Add(new XElement("Description", offer.Product.Description));
                xDescriptions.Add(xProductDesc);
            }

            xDoc.Add(xDescriptions);
            xDoc.Save("d://ProductDesc.xml");
        }

        public static void SaveRequestXML(ArchivedRequest request, Stream stream)
        {
            XDocument xDoc = new XDocument();
            XElement xRoot = new XElement("OrderFromB2BMarket");
            xRoot.Add(new XAttribute("Version", "1.0"));
            xRoot.Add(new XAttribute("Code", request.Code));
            xRoot.Add(new XAttribute("TimeOfCreation", request.DateTimeSent));

            XElement xClientInfo = new XElement("ClientInformation");
            xClientInfo.Add(new XElement("Name", request.Client.FullName));
            xClientInfo.Add(new XElement("BIN", request.Client.BIN));
            xClientInfo.Add(new XElement("Country", request.Client.Country));
            xClientInfo.Add(new XElement("City", request.Client.City));
            xClientInfo.Add(new XElement("Address", request.Client.Address));
            xClientInfo.Add(new XElement("Phone", request.Client.Phone));
            xClientInfo.Add(new XElement("EMail", request.Client.Email));
            xClientInfo.Add(new XElement("ContactPersonName", request.Client.ContactPersonName));
            xClientInfo.Add(new XElement("ContactPersonPhone", request.Client.ContactPersonPhone));

            XElement xDeliveryInfo = new XElement("DeliveryInformation");
            xDeliveryInfo.Add(new XElement("DeliveryTime", request.DeliveryTime));
            xDeliveryInfo.Add(new XElement("Comments", request.Comments));

            XElement xOrders = new XElement("Orders");
            xOrders.Add(new XAttribute("ItemsQuantity", request.ItemsQuantity));
            xOrders.Add(new XAttribute("ProductsQuantity", request.ProductsQuantity));
            xOrders.Add(new XAttribute("TotalPrice", request.TotalPrice));

            foreach (ArchivedOrder order in request.Orders)
            {
                XElement xOrder = new XElement("Order");

                XElement xProduct = new XElement("Product");
                xProduct.Add(new XAttribute("SupplierProductCode", order.SupplierProductCode));
                xProduct.Add(new XElement("Name", order.Product.Name));
                xProduct.Add(new XElement("Category", order.Product.Category.Name));
                xProduct.Add(new XElement("VolumeType", order.Product.VolumeType.Name));

                XElement xVolumeUnit = new XElement("VolumeUnit");
                xVolumeUnit.Add(new XAttribute("ShortName", order.Product.VolumeUnit.ShortName));
                xVolumeUnit.Add(new XAttribute("FullName", order.Product.VolumeUnit.FullName));
                xProduct.Add(xVolumeUnit);
                xProduct.Add(new XElement("Volume", order.Product.Volume));

                XElement xProductExtraProperties = new XElement("ExtraProperties");
                foreach (ProductExtraProperty productExtraProperty in order.Product.ExtraProperties)
                {
                    XElement xProductExtraProperty = new XElement("ExtraProperty");

                    xProductExtraProperty.Add(new XAttribute("Type", productExtraProperty.PropertyType));
                    xProductExtraProperty.Add(new XAttribute("Value", productExtraProperty.Value));
                    xProductExtraProperties.Add(xProductExtraProperty);
                }
                xProduct.Add(xProductExtraProperties);

                xOrder.Add(xProduct);
                xOrder.Add(new XElement("QuantityUnit", order.QuantityUnit));
                xOrder.Add(new XElement("Quantity", order.Quantity));
                xOrder.Add(new XElement("PricePerItem", order.Price));
                xOrder.Add(new XElement("TotalPrice", order.Quantity * order.Price));

                xOrders.Add(xOrder);
            }

            xRoot.Add(xClientInfo);
            xRoot.Add(xDeliveryInfo);
            xRoot.Add(xOrders);
            xDoc.Add(xRoot);
            xDoc.Save(stream);
        }

    }
}
