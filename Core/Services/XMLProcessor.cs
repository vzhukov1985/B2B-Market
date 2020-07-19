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
    public class MatchProductExtraPropertiesComparer : IEqualityComparer<MatchProductExtraProperty>
    {
        public bool Equals(MatchProductExtraProperty x, MatchProductExtraProperty y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.MatchProductExtraPropertyTypeId == y.MatchProductExtraPropertyTypeId &&
                x.Value == y.Value;


        }

        public int GetHashCode(MatchProductExtraProperty obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            int hashMatchProductExtraPropertyTypeId = obj.MatchProductExtraPropertyTypeId == null? 0: obj.MatchProductExtraPropertyTypeId.GetHashCode();

            int hashValue = obj.Value == null ? 0 : obj.Value.GetHashCode();

            return hashMatchProductExtraPropertyTypeId ^ hashValue;
        }
    }

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
            List<Guid> offersProcessedIds = new List<Guid>();
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

                IEnumerable<MatchProductExtraPropertyType> matchExtraPropertyTypes = db.MatchProductExtraPropertyTypes.Where(mept => mept.SupplierId == supplier.Id);
                IEnumerable<MatchProductCategory> matchProductCategories = db.MatchProductCategories.Where(mpc => mpc.SupplierId == supplier.Id);
                IEnumerable<MatchQuantityUnit> matchQuantityUnits = db.MatchQuantityUnits.Where(mqu => mqu.SupplierId == supplier.Id);
                IEnumerable<MatchVolumeType> matchVolumeTypes = db.MatchVolumeTypes.Where(mvt => mvt.SupplierId == supplier.Id);
                IEnumerable<MatchVolumeUnit> matchVolumeUnits = db.MatchVolumeUnits.Where(mvu => mvu.SupplierId == supplier.Id);
                IEnumerable<MatchOffer> matchOffers = db.MatchOffers.Where(mo => mo.SupplierId == supplier.Id);

                int newOffersAdded = 0;
                int newExtraPropertiesAdded = 0;
                int newProductCategoriesAdded = 0;
                int newVolumeTypesAdded = 0;
                int newVolumeUnitsAdded = 0;
                int newQuantityUnitsAdded = 0;

                try
                {
                    foreach (XElement xOffer in xOffers)
                    {

                        XElement xProduct = xOffer.Element("Product");

                        XElement xCategory = xProduct.Element("Category");
                        MatchProductCategory matchProductCategory = matchProductCategories.Where(mpc => mpc.SupplierCategoryName == xCategory.Value).FirstOrDefault();
                        if (matchProductCategory == null)
                        {
                            ProductCategory existingProductCategory = productCategories.Where(pc => pc.Name == xCategory.Value).FirstOrDefault();
                            matchProductCategory = new MatchProductCategory { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierCategoryName = xCategory.Value, ProductCategoryId = existingProductCategory == null ? (Guid?)null : existingProductCategory.Id };
                            db.MatchProductCategories.Add(matchProductCategory);
                            newProductCategoriesAdded++;
                        }

                        XElement xVolumeType = xProduct.Element("VolumeType");
                        MatchVolumeType matchVolumeType = matchVolumeTypes.Where(mvt => mvt.SupplierVolumeTypeName == xVolumeType.Value).FirstOrDefault();
                        if (matchVolumeType == null)
                        {
                            VolumeType existingVolumeType = volumeTypes.Where(vt => vt.Name == xVolumeType.Value).FirstOrDefault();
                            matchVolumeType = new MatchVolumeType { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierVolumeTypeName = xVolumeType.Value, VolumeTypeId = existingVolumeType == null ? (Guid?)null : existingVolumeType.Id };
                            db.MatchVolumeTypes.Add(matchVolumeType);
                            newVolumeTypesAdded++;
                        }

                        XElement xVolumeUnit = xProduct.Element("VolumeUnit");
                        MatchVolumeUnit matchVolumeUnit = matchVolumeUnits.Where(mvu => mvu.SupplierVUShortName == xVolumeUnit.Attribute("ShortName").Value && mvu.SupplierVUFullName == xVolumeUnit.Attribute("FullName").Value).FirstOrDefault();
                        if (matchVolumeUnit == null)
                        {
                            VolumeUnit existingVolumeUnit = volumeUnits.Where(vu => vu.ShortName == xVolumeUnit.Attribute("ShortName").Value && vu.FullName == xVolumeUnit.Attribute("FullName").Value).FirstOrDefault();
                            matchVolumeUnit = new MatchVolumeUnit { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierVUShortName = xVolumeUnit.Attribute("ShortName").Value, SupplierVUFullName = xVolumeUnit.Attribute("FullName").Value, VolumeUnitId = existingVolumeUnit == null ? (Guid?)null : existingVolumeUnit.Id };
                            db.MatchVolumeUnits.Add(matchVolumeUnit);
                            newVolumeUnitsAdded++;
                        }


                        XElement xQuantityUnit = xOffer.Element("QuantityUnit");
                        MatchQuantityUnit matchQuantityUnit = matchQuantityUnits.Where(mqu => mqu.SupplierQUShortName == xQuantityUnit.Attribute("ShortName").Value && mqu.SupplierQUFullName == xQuantityUnit.Attribute("FullName").Value).FirstOrDefault();
                        if (matchQuantityUnit == null)
                        {
                            QuantityUnit existingQuantityUnit = quantityUnits.Where(qu => qu.ShortName == xQuantityUnit.Attribute("ShortName").Value && qu.FullName == xQuantityUnit.Attribute("FullName").Value).FirstOrDefault();
                            matchQuantityUnit = new MatchQuantityUnit { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierQUShortName = xQuantityUnit.Attribute("ShortName").Value, SupplierQUFullName = xQuantityUnit.Attribute("FullName").Value, QuantityUnitId = existingQuantityUnit == null ? (Guid?)null : existingQuantityUnit.Id };
                            db.MatchQuantityUnits.Add(matchQuantityUnit);
                            newQuantityUnitsAdded++;
                        }


                        List<MatchProductExtraProperty> productExtraProperties = new List<MatchProductExtraProperty>();

                        IEnumerable<XElement> xExtraProperties = xProduct.Element("ExtraProperties").Elements("ExtraProperty");
                        foreach (XElement xExtraProperty in xExtraProperties)
                        {
                            MatchProductExtraPropertyType matchProductExtraPropertyType = matchExtraPropertyTypes.Where(mept => mept.SupplierProductExtraPropertyTypeName == xExtraProperty.Attribute("Type").Value).FirstOrDefault();
                            if (matchProductExtraPropertyType == null)
                            {
                                ProductExtraPropertyType existingExtraPropertyType = extraPropertyTypes.Where(ept => ept.Name == xExtraProperty.Attribute("Type").Value).FirstOrDefault();
                                matchProductExtraPropertyType = new MatchProductExtraPropertyType { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierProductExtraPropertyTypeName = xExtraProperty.Attribute("Type").Value, ProductExtraPropertyTypeId = existingExtraPropertyType == null ? (Guid?)null : existingExtraPropertyType.Id };
                                db.MatchProductExtraPropertyTypes.Add(matchProductExtraPropertyType);
                                newExtraPropertiesAdded++;
                            }
                            productExtraProperties.Add(new MatchProductExtraProperty { Id = Guid.NewGuid(), MatchProductExtraPropertyTypeId = matchProductExtraPropertyType.Id, Value = xExtraProperty.Attribute("Value").Value });
                        }

                        productExtraProperties = productExtraProperties.OrderBy(pep => pep.MatchProductExtraPropertyTypeId).ToList();

                        db.SaveChanges();

                        MatchOffer matchOfferExists = matchOffers.Where(of => of.SupplierProductCode == xOffer.Attribute("SupplierProductCode").Value).FirstOrDefault();
                        bool addNewMatchOffer = false;

                        if (matchOfferExists == null)
                        {
                            addNewMatchOffer = true;
                        }
                        else
                        {
                            bool offerFullyMatches = true;
                            Guid MatchProductCategoryIdToCompare = matchProductCategories.Where(mpc => mpc.SupplierCategoryName == xProduct.Element("Category").Value).Select(mpc => mpc.Id).FirstOrDefault();
                            Guid MatchQuantityUnitIdToCompare = matchQuantityUnits.Where(mqu => mqu.SupplierQUShortName == xQuantityUnit.Attribute("ShortName").Value && mqu.SupplierQUFullName == xQuantityUnit.Attribute("FullName").Value).Select(mqu => mqu.Id).FirstOrDefault();
                            Guid MatchVolumeTypeIdToCompare = matchVolumeTypes.Where(mvt => mvt.SupplierVolumeTypeName == xVolumeType.Value).Select(mvt => mvt.Id).FirstOrDefault();
                            Guid MatchVolumeUnitIdToCompare = matchVolumeUnits.Where(mvu => mvu.SupplierVUShortName == xVolumeUnit.Attribute("ShortName").Value && mvu.SupplierVUFullName == xVolumeUnit.Attribute("FullName").Value).Select(mvu => mvu.Id).FirstOrDefault();
                            if (matchOfferExists.MatchProductCategoryId != MatchProductCategoryIdToCompare ||
                                matchOfferExists.MatchQuantityUnitId != MatchQuantityUnitIdToCompare ||
                                matchOfferExists.MatchVolumeTypeId != MatchVolumeTypeIdToCompare ||
                                matchOfferExists.MatchVolumeUnitId != MatchVolumeUnitIdToCompare ||
                                matchOfferExists.ProductName != xProduct.Element("Name").Value ||
                                matchOfferExists.ProductVolume != Convert.ToDecimal(xProduct.Element("Volume").Value, new CultureInfo("en-US")))
                            {
                                offerFullyMatches = false;
                            }

                            List<MatchProductExtraProperty> matchProductExtraPropertiesToCompare = db.MatchProductExtraProperties.Where(mpep => mpep.MatchOfferId == matchOfferExists.Id).ToList();
                            matchProductExtraPropertiesToCompare = matchProductExtraPropertiesToCompare.OrderBy(mpep => mpep.MatchProductExtraPropertyTypeId).ToList();
                            if (productExtraProperties.Count != matchProductExtraPropertiesToCompare.Count)
                            {
                                offerFullyMatches = false;
                            }
                            else
                            {
                                offerFullyMatches = productExtraProperties.SequenceEqual(matchProductExtraPropertiesToCompare, new MatchProductExtraPropertiesComparer());
                            }


                            if (offerFullyMatches == true)
                            {
                                Offer correspondingOffer;
                                if (matchOfferExists.OfferId != null)
                                {
                                    correspondingOffer = db.Offers.Find(matchOfferExists.OfferId);
                                    correspondingOffer.Remains = Convert.ToInt32(xOffer.Element("Remains").Value);
                                    correspondingOffer.RetailPrice = Convert.ToDecimal(xOffer.Element("RetailPrice").Value, new CultureInfo("en-US"));
                                    correspondingOffer.DiscountPrice = Convert.ToDecimal(xOffer.Element("DiscountPrice").Value, new CultureInfo("en-US"));
                                    db.Offers.Update(correspondingOffer);
                                }
                                else
                                {
                                    matchOfferExists.Remains = Convert.ToInt32(xOffer.Element("Remains").Value);
                                    matchOfferExists.RetailPrice = Convert.ToDecimal(xOffer.Element("RetailPrice").Value, new CultureInfo("en-US"));
                                    matchOfferExists.DiscountPrice = Convert.ToDecimal(xOffer.Element("DiscountPrice").Value, new CultureInfo("en-US"));
                                    db.MatchOffers.Update(matchOfferExists);
                                }
                            }
                            else
                            {
                                Offer correspondingOffer;
                                if (matchOfferExists.OfferId != null)
                                {
                                    correspondingOffer = db.Offers.Find(matchOfferExists.OfferId);
                                    db.Offers.Remove(correspondingOffer);
                                }

                                IEnumerable<MatchProductExtraProperty> pepsToRemove = db.MatchProductExtraProperties.Where(mpep => mpep.MatchOfferId == matchOfferExists.Id);
                                db.MatchProductExtraProperties.RemoveRange(pepsToRemove);
                                db.SaveChanges();
                                db.MatchOffers.Remove(matchOfferExists);
                                db.SaveChanges();

                                addNewMatchOffer = true;
                            }

                        }

                        if (addNewMatchOffer)
                        {
                            MatchOffer newMatchOffer = new MatchOffer
                            {
                                Id = Guid.NewGuid(),
                                SupplierId = supplier.Id,
                                SupplierProductCode = xOffer.Attribute("SupplierProductCode").Value,
                                MatchProductCategoryId = matchProductCategory.Id,
                                MatchQuantityUnitId = matchQuantityUnit.Id,
                                MatchVolumeTypeId = matchVolumeType.Id,
                                MatchVolumeUnitId = matchVolumeUnit.Id,
                                ProductName = xProduct.Element("Name").Value,
                                ProductVolume = Convert.ToDecimal(xProduct.Element("Volume").Value, new CultureInfo("en-US")),
                                Remains = Convert.ToInt32(xOffer.Element("Remains").Value),
                                RetailPrice = Convert.ToDecimal(xOffer.Element("RetailPrice").Value, new CultureInfo("en-US")),
                                DiscountPrice = Convert.ToDecimal(xOffer.Element("DiscountPrice").Value, new CultureInfo("en-US"))
                            };

                            db.MatchOffers.Add(newMatchOffer);
                            matchOfferExists = newMatchOffer;
                            newOffersAdded++;

                            foreach (MatchProductExtraProperty matchProductExtraProperty in productExtraProperties)
                            {
                                MatchProductExtraProperty newExtraProperty = new MatchProductExtraProperty
                                {
                                    Id = matchProductExtraProperty.Id,
                                    MatchOfferId = newMatchOffer.Id,
                                    MatchProductExtraPropertyTypeId = matchProductExtraProperty.MatchProductExtraPropertyTypeId,
                                    Value = matchProductExtraProperty.Value
                                };
                                db.MatchProductExtraProperties.Add(newExtraProperty);
                            }
                        }

                        offersProcessedIds.Add(matchOfferExists.Id);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    logFileStream.WriteLine("{0} - ERROR: EXCEPTION \"{1}\"", DateTime.Now.ToString("G"), e.Message);
                    return;
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
                if (newOffersAdded > 0)
                    logFileStream.WriteLine("{0} - {1} new offers were added", DateTime.Now.ToString("G"), newOffersAdded.ToString());
            }

            //Remove all info regarding offers that are in Db but doesn't exist in current extraction
            try
            {
                int matchOffersDeleted = 0;
                int existingOffersDeleted = 0;
                using (MarketDbContext db = new MarketDbContext())
                {
                    List<Guid> allExistingMatchOffersIds = db.MatchOffers.Where(mo => mo.SupplierId == supplier.Id).Select(mo => mo.Id).ToList();
                    List<Guid> matchOffersToRemoveIds = allExistingMatchOffersIds.Except(offersProcessedIds).ToList();
                    foreach (Guid matchOfferToRemoveId in matchOffersToRemoveIds)
                    {
                        MatchOffer matchOfferToRemove = db.MatchOffers.Find(matchOfferToRemoveId);
                        if (matchOfferToRemove.OfferId != null)
                        {
                            db.CurrentOrders.RemoveRange(db.CurrentOrders.Where(co => co.OfferId == matchOfferToRemove.OfferId));
                            db.SaveChanges();
                            db.Offers.RemoveRange(db.Offers.Where(o => o.Id == matchOfferToRemove.OfferId).FirstOrDefault());
                            db.SaveChanges();
                            existingOffersDeleted++;
                        }

                        db.MatchProductExtraProperties.RemoveRange(db.MatchProductExtraProperties.Where(mpep => mpep.MatchOfferId == matchOfferToRemove.Id));
                        db.SaveChanges();
                        db.MatchOffers.Remove(matchOfferToRemove);
                        db.SaveChanges();
                        matchOffersDeleted++;
                    }
                }

                if (matchOffersDeleted > 0)
                    logFileStream.WriteLine("{0} - {1} matching offers and {3} active offers corresponding to them were removed", DateTime.Now.ToString("G"), matchOffersDeleted.ToString(), existingOffersDeleted.ToString());
            }
            catch (Exception e)
            {
                logFileStream.WriteLine("{0} - ERROR: EXCEPTION \"{1}\"", DateTime.Now.ToString("G"), e.Message);
                return;
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
