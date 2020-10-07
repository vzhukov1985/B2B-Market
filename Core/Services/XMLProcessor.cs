using Core.DBModels;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Core.Services
{
    public class XMLProcessor
    {
        //Obsolete func
        /*       public static void ExtractAllProductsToXML(string fileName, Guid supplierId)
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
                   xSupplierInfo.Add(new XElement("Country", supplier.Country));
                   xSupplierInfo.Add(new XElement("City", supplier.City));
                   xSupplierInfo.Add(new XElement("Address", supplier.Address));
                   xSupplierInfo.Add(new XElement("Phone", supplier.Phone));
                   xSupplierInfo.Add(new XElement("EMail", supplier.Email));
                   xSupplierInfo.Add(new XElement("ContactPersonName", supplier.ContactPersonName));
                   xSupplierInfo.Add(new XElement("ContactPersonPhone", supplier.ContactPersonPhone));

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
               }*/

        public static MemoryStream CreateReqProdPics(Offer offer)
        {
            XDocument xDoc = new XDocument();

            XElement xRequests = new XElement("requestproductspictures");
            xRequests.Add(new XAttribute("version", "1.0"));
            xRequests.Add(new XAttribute("supplierid", offer.Supplier.Id));

            XElement xRequest = new XElement("request");
            xRequest.Add(new XAttribute("supplierproductcode", offer.SupplierProductCode));
            xRequests.Add(xRequest);

            xDoc.Add(xRequests);

            MemoryStream resStream = new MemoryStream();
            xDoc.Save(resStream);
            return resStream;
        }

        public static MemoryStream UpdateReqProdPics(Stream xmlReadStream, Offer offer)
        {
            XDocument xDoc = XDocument.Load(xmlReadStream);

            XElement xRequests = xDoc.Element("requestproductspictures");

            foreach (XElement xRequest in xRequests.Elements())
            {
                if (xRequest.Attribute("supplierproductcode").Value == offer.SupplierProductCode)
                    return null;
            }
            XElement xNewRequest = new XElement("request");
            xNewRequest.Add(new XAttribute("supplierproductcode", offer.SupplierProductCode));
            xRequests.Add(xNewRequest);

            MemoryStream resStream = new MemoryStream();
            xDoc.Save(resStream);
            return resStream;
        }

        public static MemoryStream CreateReqProdDesc(Offer offer)
        {
            XDocument xDoc = new XDocument();

            XElement xRequests = new XElement("requestproductsdescriptions");
            xRequests.Add(new XAttribute("version", "1.0"));
            xRequests.Add(new XAttribute("supplierid", offer.Supplier.Id));

            XElement xRequest = new XElement("request");
            xRequest.Add(new XAttribute("supplierproductcode", offer.SupplierProductCode));
            xRequests.Add(xRequest);

            xDoc.Add(xRequests);

            MemoryStream resStream = new MemoryStream();
            xDoc.Save(resStream);
            return resStream;
        }

        public static MemoryStream UpdateReqProdDesc(Stream xmlReadStream, Offer offer)
        {
            XDocument xDoc = XDocument.Load(xmlReadStream);

            XElement xRequests = xDoc.Element("requestproductsdescriptions");

            foreach (XElement xRequest in xRequests.Elements())
            {
                if (xRequest.Attribute("supplierproductcode").Value == offer.SupplierProductCode)
                    return null;
            }
            XElement xNewRequest = new XElement("request");
            xNewRequest.Add(new XAttribute("supplierproductcode", offer.SupplierProductCode));
            xRequests.Add(xNewRequest);

            MemoryStream resStream = new MemoryStream();
            xDoc.Save(resStream);
            return resStream;
        }

        public static void SaveRequestXMLToStream(ArchivedRequest request, Stream stream)
        {
            XDocument xDoc = new XDocument();
            XElement xRoot = new XElement("orderfromb2bmarket");
                xRoot.Add(new XAttribute("version", "1.0"));
                xRoot.Add(new XAttribute("code", request.Code));
                xRoot.Add(new XAttribute("timeofcreation", request.DateTimeSent));

            XElement xClientInfo = new XElement("clientinformation");
                xClientInfo.Add(new XElement("name", request.Client.FullName));
                xClientInfo.Add(new XElement("bin", request.Client.Bin));
                xClientInfo.Add(new XElement("country", request.Client.Country));
                xClientInfo.Add(new XElement("city", request.Client.City));
                xClientInfo.Add(new XElement("address", request.Client.Address));
                xClientInfo.Add(new XElement("phone", request.Client.Phone));
                xClientInfo.Add(new XElement("email", request.Client.Email));
                xClientInfo.Add(new XElement("contactpersonname", request.Client.ContactPersonName));
                xClientInfo.Add(new XElement("contactpersonphone", request.Client.ContactPersonPhone));

            XElement xDeliveryInfo = new XElement("deliveryinformation");
                xDeliveryInfo.Add(new XElement("deliverytime", request.DeliveryDateTime));
                xDeliveryInfo.Add(new XElement("comments", request.Comments));

            XElement xOrders = new XElement("orders");
                xOrders.Add(new XAttribute("itemsquantity", request.ItemsQuantity));
                xOrders.Add(new XAttribute("productsquantity", request.ProductsQuantity));
                xOrders.Add(new XAttribute("totalprice", request.TotalPrice));

            foreach (ArchivedOrder order in request.ArchivedOrders)
            {
                XElement xOrder = new XElement("order");

                XElement xProduct = new XElement("product");
                    xProduct.Add(new XAttribute("supplierproductcode", order.SupplierProductCode));
                    xProduct.Add(new XElement("name", order.ProductName));
                    xProduct.Add(new XElement("category", order.ProductCategory));
                    xProduct.Add(new XElement("volumetype", order.ProductVolumeType));
                    xProduct.Add(new XElement("volumeunit", order.ProductVolumeUnit));
                    xProduct.Add(new XElement("volume", order.ProductVolume));

                xOrder.Add(xProduct);
                xOrder.Add(new XElement("quantityunit", order.QuantityUnit));
                xOrder.Add(new XElement("quantity", order.Quantity));
                xOrder.Add(new XElement("priceperitem", order.Price));
                xOrder.Add(new XElement("totalprice", order.Quantity * order.Price));
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
