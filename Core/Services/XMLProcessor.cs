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
        }

        public static void ProcessOffersXMLFromFile(string xmlFile, StreamWriter logFileStream)
        {
            XDocument xDoc = XDocument.Load(xmlFile);
            XElement xExtraction = xDoc.Root;
            XElement xSupplierInfo = xExtraction.Element("SupplierInfo");

            Supplier supplier;

            using (MarketDbContext db = new MarketDbContext())
            {
                try
                {
                    supplier = db.Suppliers.Find(new Guid(xSupplierInfo.Attribute("Id").Value));
                    if (supplier == null)
                    {
                        logFileStream.WriteLine("{0} - ERROR: Id \"{1}\" of supplier wasn't found in DB", DateTime.Now.ToString("G"), xSupplierInfo.Attribute("Id").Value);
                        return;
                    }
                    supplier.ShortName = xSupplierInfo.Element("ShortName").Value;
                    supplier.FullName = xSupplierInfo.Element("FullName").Value;
                    supplier.BIN = xSupplierInfo.Element("BIN").Value;
                    supplier.Country = xSupplierInfo.Element("Country").Value;
                    supplier.City = xSupplierInfo.Element("City").Value;
                    supplier.Address = xSupplierInfo.Element("Address").Value;
                    supplier.Phone = xSupplierInfo.Element("Phone").Value;
                    supplier.Email = xSupplierInfo.Element("EMail").Value;
                    supplier.ContactPersonName = xSupplierInfo.Element("ContactPersonName").Value;
                    supplier.ContactPersonPhone = xSupplierInfo.Element("ContactPersonPhone").Value;
                    db.Suppliers.Update(supplier);
                    if (db.SaveChanges() > 0)
                        logFileStream.WriteLine("{0} - Info for supplier \"{1}\" was updated", DateTime.Now.ToString("G"), supplier.ShortName);
                }
                catch (Exception e)
                {
                    logFileStream.WriteLine("{0} - ERROR: EXCEPTION \"{1}\"", DateTime.Now.ToString("G"), e.Message);
                    return;
                }

                IEnumerable<XElement> xOffers = xExtraction.Element("Offers").Elements("Offer");

                IEnumerable<QuantityUnit> quantityUnits = db.QuantityUnits;
                IEnumerable<ProductCategory> productCategories = db.ProductCategories;
                IEnumerable<Offer> offers = db.Offers.Where(o => o.SupplierId == supplier.Id);
                IEnumerable<Product> products = db.Products;
                IEnumerable<ProductExtraPropertyType> extraPropertyTypes = db.ProductExtraPropertyTypes;
                IEnumerable<ProductExtraProperty> extraProperties = db.ProductExtraProperties;
                IEnumerable<VolumeType> volumeTypes = db.VolumeTypes;
                IEnumerable<VolumeUnit> volumeUnits = db.VolumeUnits;

                IEnumerable<MatchExtraPropertyType> matchExtraPropertyTypes = db.MatchExtraPropertyTypes.Where(mept => mept.SupplierId == supplier.Id);
                IEnumerable<MatchProduct> matchProducts = db.MatchProducts.Where(mp => mp.SupplierId == supplier.Id);
                IEnumerable<MatchProductCategory> matchProductCategories = db.MatchProductCategories.Where(mpc => mpc.SupplierId == supplier.Id);
                IEnumerable<MatchQuantityUnit> matchQuantityUnits = db.MatchQuantityUnits.Where(mqu => mqu.SupplierId == supplier.Id);
                IEnumerable<MatchVolumeType> matchVolumeTypes = db.MatchVolumeTypes.Where(mvt => mvt.SupplierId == supplier.Id);
                IEnumerable<MatchVolumeUnit> matchVolumeUnits = db.MatchVolumeUnits.Where(mvu => mvu.SupplierId == supplier.Id);

                int newProductsAdded = 0;
                int newExtraPropertiesAdded = 0;
                int newProductCategoriesAdded = 0;
                int newVolumeTypesAdded = 0;
                int newVolumeUnitsAdded = 0;
                int newQuantityUnitsAdded = 0;

                foreach (XElement xOffer in xOffers)
                {
                    try
                    {
                        XElement xProduct = xOffer.Element("Product");

                        XElement xCategory = xProduct.Element("Category");
                        MatchProductCategory matchProductCategory = matchProductCategories.Where(mpc => mpc.SupplierCategoryName == xCategory.Value).FirstOrDefault();
                        ProductCategory productCategory;
                        if (matchProductCategory == null)
                        {
                            productCategory = productCategories.Where(pc => pc.Name == xCategory.Value).FirstOrDefault();
                            if (productCategory == null)
                            {
                                productCategory = new ProductCategory { Id = new Guid(), Name = xCategory.Value, MidCategoryId = new Guid("ccad6dcb-6dc8-4f62-a7ae-a904013e772a"), IsChecked = false }; //МидКатегория "Нет"
                                db.ProductCategories.Add(productCategory);
                                db.SaveChanges();
                                newProductCategoriesAdded++;
                            }
                            db.MatchProductCategories.Add(new MatchProductCategory { SupplierId = supplier.Id, ProductCategoryId = productCategory.Id, SupplierCategoryName = xCategory.Value, IsChecked = false });
                        }
                        else
                        {
                            productCategory = productCategories.Where(pc => pc.Id == matchProductCategory.ProductCategoryId).FirstOrDefault();
                        }
                        

                        XElement xVolumeType = xProduct.Element("VolumeType");
                        MatchVolumeType matchVolumeType = matchVolumeTypes.Where(vt => vt.SupplierVolumeTypeName == xVolumeType.Value).FirstOrDefault();
                        VolumeType volumeType;
                        if (matchVolumeType == null)
                        {
                            volumeType = volumeTypes.Where(vt => vt.Name == xVolumeType.Value).FirstOrDefault();
                            if (volumeType == null)
                            {
                                volumeType = new VolumeType { Id = new Guid(), Name = xVolumeType.Value, IsChecked = false };
                                db.VolumeTypes.Add(volumeType);
                                db.SaveChanges();
                                newVolumeTypesAdded++;
                            }
                            db.MatchVolumeTypes.Add(new MatchVolumeType { SupplierId = supplier.Id, SupplierVolumeTypeName = xVolumeType.Value, VolumeTypeId = volumeType.Id, IsChecked = false });
                        }
                        else
                        {
                            volumeType = volumeTypes.Where(vt => vt.Id == matchVolumeType.VolumeTypeId).FirstOrDefault();
                        }
                        

                        XElement xVolumeUnit = xProduct.Element("VolumeUnit");
                        MatchVolumeUnit matchVolumeUnit = matchVolumeUnits.Where(mvu => mvu.SupplierVUShortName == xVolumeUnit.Attribute("ShortName").Value && mvu.SupplierVUFullName == xVolumeUnit.Attribute("FullName").Value).FirstOrDefault();
                        VolumeUnit volumeUnit;
                        if (matchVolumeUnit == null)
                        {
                            volumeUnit = volumeUnits.Where(vu => vu.ShortName == xVolumeUnit.Attribute("ShortName").Value && vu.FullName == xVolumeUnit.Attribute("FullName").Value).FirstOrDefault();
                            if (volumeUnit == null)
                            {
                                volumeUnit = new VolumeUnit { Id = new Guid(), ShortName = xVolumeUnit.Attribute("ShortName").Value, FullName = xVolumeUnit.Attribute("FullName").Value, IsChecked = false };
                                db.VolumeUnits.Add(volumeUnit);
                                db.SaveChanges();
                                newVolumeUnitsAdded++;
                            }
                            db.MatchVolumeUnits.Add(new MatchVolumeUnit { SupplierId = supplier.Id, SupplierVUShortName = xVolumeUnit.Attribute("ShortName").Value, SupplierVUFullName = xVolumeUnit.Attribute("FullName").Value, VolumeUnitId = volumeUnit.Id, IsChecked = false });
                        }
                        else
                        {
                            volumeUnit = volumeUnits.Where(vu => vu.Id == matchVolumeUnit.VolumeUnitId).FirstOrDefault();
                        }
                        

                        XElement xQuantityUnit = xOffer.Element("QuantityUnit");

                        MatchQuantityUnit matchQuantityUnit = matchQuantityUnits.Where(mqu => mqu.SupplierQUShortName == xQuantityUnit.Attribute("ShortName").Value && mqu.SupplierQUFullName == xQuantityUnit.Attribute("FullName").Value).FirstOrDefault();
                        QuantityUnit quantityUnit;
                        if (matchQuantityUnit == null)
                        {
                            quantityUnit = quantityUnits.Where(qu => qu.ShortName == xQuantityUnit.Attribute("ShortName").Value && qu.FullName == xQuantityUnit.Attribute("FullName").Value).FirstOrDefault();
                            if (quantityUnit == null)
                            {
                                quantityUnit = new QuantityUnit { Id = new Guid(), ShortName = xQuantityUnit.Attribute("ShortName").Value, FullName = xQuantityUnit.Attribute("FullName").Value, IsChecked = false };
                                db.QuantityUnits.Add(quantityUnit);
                                db.SaveChanges();
                                newQuantityUnitsAdded++;
                            }
                            db.MatchQuantityUnits.Add(new MatchQuantityUnit { SupplierId = supplier.Id, SupplierQUShortName = xQuantityUnit.Attribute("ShortName").Value, SupplierQUFullName = xQuantityUnit.Attribute("FullName").Value, QuantityUnitId = quantityUnit.Id, IsChecked = false });
                        }
                        else
                        {
                            quantityUnit = quantityUnits.Where(qu => qu.Id == matchQuantityUnit.QuantityUnitId).FirstOrDefault();
                        }

                        List<ProductExtraProperty> productExtraProperties = new List<ProductExtraProperty>();

                        IEnumerable<XElement> xExtraProperties = xProduct.Element("ExtraProperties").Elements("ExtraProperty");
                        foreach (XElement xExtraProperty in xExtraProperties)
                        {
                            MatchExtraPropertyType matchExtraPropertyType = matchExtraPropertyTypes.Where(mept => mept.SupplierEPTypeName == xExtraProperty.Attribute("Type").Value).FirstOrDefault();
                            ProductExtraPropertyType productExtraPropertyType;
                            if (matchExtraPropertyType == null)
                            {
                                productExtraPropertyType = extraPropertyTypes.Where(ept => ept.Name == xExtraProperty.Attribute("Type").Value).FirstOrDefault();
                                if (productExtraPropertyType == null)
                                {
                                    productExtraPropertyType = new ProductExtraPropertyType { Id = new Guid(), Name = xExtraProperty.Attribute("Type").Value, IsChecked = false };
                                    db.ProductExtraPropertyTypes.Add(productExtraPropertyType);
                                    db.SaveChanges();
                                    newExtraPropertiesAdded++;
                                }
                                db.MatchExtraPropertyTypes.Add(new MatchExtraPropertyType { SupplierId = supplier.Id, SupplierEPTypeName = xExtraProperty.Attribute("Type").Value, ExtraPropertyTypeId = productExtraPropertyType.Id, IsChecked = false });
                            }
                            else
                            {
                                productExtraPropertyType = extraPropertyTypes.Where(ept => ept.Id == matchExtraPropertyType.ExtraPropertyTypeId).FirstOrDefault();
                            }
                            productExtraProperties.Add(new ProductExtraProperty { ProductId = new Guid(), Product = null, PropertyType = null, PropertyTypeId = productExtraPropertyType.Id, Value = xExtraProperty.Attribute("Value").Value });
                        }

                        Product product;
                        MatchProduct matchProduct = matchProducts.Where(mp => mp.SupplierId == supplier.Id && mp.SupplierProductCode == xOffer.Attribute("SupplierProductCode").Value).FirstOrDefault();
                        if (matchProduct == null)
                        {
                            product = new Product { Id = new Guid(), CategoryId = productCategory.Id, Name = xProduct.Element("Name").Value, VolumeTypeId = volumeType.Id, VolumeUnitId = volumeUnit.Id, Volume = Convert.ToDecimal(xProduct.Element("Volume").Value, new System.Globalization.CultureInfo("en-US")), IsChecked = false };
                            db.Products.Add(product);
                            db.SaveChanges();
                            db.MatchProducts.Add(new MatchProduct { SupplierId = supplier.Id, SupplierProductCode = xOffer.Attribute("SupplierProductCode").Value, ProductId = product.Id, IsChecked = false });
                            newProductsAdded++;
                        }
                        else
                        {
                            product = products.Where(p => p.Id == matchProduct.ProductId).FirstOrDefault();
                        }

                        foreach (ProductExtraProperty productExtraProperty in productExtraProperties)
                        {
                            ProductExtraProperty property = extraProperties.Where(ep => ep.ProductId == product.Id && ep.PropertyTypeId == productExtraProperty.PropertyTypeId).FirstOrDefault();
                            if (property == null)
                            {
                                productExtraProperty.ProductId = product.Id;
                                db.ProductExtraProperties.Add(productExtraProperty);
                            }
                            else
                            {
                                property.Value = productExtraProperty.Value;
                                db.ProductExtraProperties.Update(property);
                            }
                        }

                        Offer offer = offers.Where(o => o.ProductId == product.Id && o.QuantityUnitId == quantityUnit.Id && o.SupplierProductCode == xOffer.Attribute("SupplierProductCode").Value).FirstOrDefault();
                        if (offer == null)
                        {
                            offer = new Offer
                            {
                                Id = new Guid(),
                                IsChecked = false,
                                QuantityUnitId = quantityUnit.Id,
                                SupplierId = supplier.Id,
                                IsActive = true,
                                SupplierProductCode = xOffer.Attribute("SupplierProductCode").Value,
                                ProductId = product.Id,
                                RetailPrice = Convert.ToDecimal(xOffer.Element("RetailPrice").Value, new CultureInfo("en-US")),
                                DiscountPrice = Convert.ToDecimal(xOffer.Element("DiscountPrice").Value, new CultureInfo("en-US")),
                                Remains = Convert.ToInt32(xOffer.Element("Remains").Value)
                            };
                            db.Offers.Add(offer);
                        }
                        else
                        {
                            offer.RetailPrice = Convert.ToDecimal(xOffer.Element("RetailPrice").Value, new CultureInfo("en-US"));
                            offer.DiscountPrice = Convert.ToDecimal(xOffer.Element("DiscountPrice").Value, new CultureInfo("en-US"));
                            offer.Remains = Convert.ToInt32(xOffer.Element("Remains").Value);
                            db.Offers.Update(offer);
                        }

                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        logFileStream.WriteLine("{0} - ERROR: EXCEPTION \"{1}\"", DateTime.Now.ToString("G"), e.Message);
                        return;
                    }
                }

                if (newProductCategoriesAdded > 0)
                    logFileStream.WriteLine("{0} - {1} new product categories were added", DateTime.Now.ToString("G"), newProductCategoriesAdded.ToString());
                if (newVolumeTypesAdded > 0)
                    logFileStream.WriteLine("{0} - {1} new volume types were added", DateTime.Now.ToString("G"), newVolumeTypesAdded.ToString());
                if (newVolumeUnitsAdded > 0)
                    logFileStream.WriteLine("{0} - {1} new volume units were added", DateTime.Now.ToString("G"), newVolumeUnitsAdded.ToString());
                if (newQuantityUnitsAdded > 0)
                    logFileStream.WriteLine("{0} - {1} new quantity units were added", DateTime.Now.ToString("G"), newQuantityUnitsAdded.ToString());
                if (newExtraPropertiesAdded > 0)
                    logFileStream.WriteLine("{0} - {1} new products' extra property types were added", DateTime.Now.ToString("G"), newExtraPropertiesAdded.ToString());
                if (newProductsAdded > 0)
                    logFileStream.WriteLine("{0} - {1} new products were added", DateTime.Now.ToString("G"), newProductsAdded.ToString());
            }

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

        public static void ProductsDescriptions(List<Offer> offers, Guid supplierId)
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

        public static void SaveRequestXMLToStream(ArchivedRequest request, Stream stream)
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
