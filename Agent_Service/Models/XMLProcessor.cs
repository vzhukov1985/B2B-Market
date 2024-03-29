﻿using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace UpdateDb_Service.Models
{
    public class MatchProductExtraPropertiesComparer : IEqualityComparer<MatchProductExtraProperty>
    {
        public bool Equals(MatchProductExtraProperty x, MatchProductExtraProperty y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return x.MatchProductExtraPropertyTypeId == y.MatchProductExtraPropertyTypeId &&
                x.Value == y.Value;


        }

        public int GetHashCode(MatchProductExtraProperty obj)
        {
            if (obj is null) return 0;

            int hashMatchProductExtraPropertyTypeId = obj.MatchProductExtraPropertyTypeId == null ? 0 : obj.MatchProductExtraPropertyTypeId.GetHashCode();

            int hashValue = obj.Value == null ? 0 : obj.Value.GetHashCode();

            return hashMatchProductExtraPropertyTypeId ^ hashValue;
        }
    }

    public class XMLProcessor
    {
        public static void ProcessOffersXMLFromFile(string xmlFile, UpdateLogger logger)
        {
            bool exceptionsFound = false;
            XDocument xDoc;
            XElement xExtraction;
            XElement xSupplierInfo;
            try
            {
                xDoc = XDocument.Load(xmlFile);
                xExtraction = xDoc.Root;
                xSupplierInfo = xExtraction.Element("supplierinfo");
            }
            catch (Exception e)
            {
                logger.Stats.Exceptions.Add(new UpdateException(e.Message, e) { CodeBlock = "Offers Extraction - Exception on XML Load", FilePath = xmlFile });
                return;
            }

            Supplier supplier;
            List<Guid> offersProcessedIds = new List<Guid>();
            using (MarketDbContext db = new MarketDbContext())
            {
                try
                {
                    supplier = db.Suppliers.Where(s => s.Id == new Guid(xSupplierInfo.Attribute("id").Value)).FirstOrDefault();
                    if (supplier == null)
                    {
                        logger.Stats.Exceptions.Add(new UpdateException($"Supplier with Id {xSupplierInfo.Attribute("id").Value} was not found")
                        {
                            FilePath = xmlFile,
                            CodeBlock = "Offers Extraction - SupplierInfo",
                        });
                        return;
                    }
                    supplier.ShortName = xSupplierInfo.Element("shortname").Value;
                    supplier.FullName = xSupplierInfo.Element("fullname").Value;
                    supplier.Bin = xSupplierInfo.Element("bin").Value;
                    supplier.Country = xSupplierInfo.Element("country").Value;
                    supplier.City = xSupplierInfo.Element("city").Value;
                    supplier.Address = xSupplierInfo.Element("address").Value;
                    supplier.Phone = xSupplierInfo.Element("phone").Value;
                    supplier.Email = xSupplierInfo.Element("email").Value;
                    supplier.ContactPersonName = xSupplierInfo.Element("contactpersonname").Value;
                    supplier.ContactPersonPhone = xSupplierInfo.Element("contactpersonphone").Value;
                    db.Suppliers.Update(supplier);
                }
                catch (Exception e)
                {
                    logger.Stats.Exceptions.Add(new UpdateException(e.Message, e) { CodeBlock = "Offers Extraction - SupplierInfo", FilePath = xmlFile });
                    return;
                }

                IEnumerable<XElement> xOffers = xExtraction.Element("offers").Elements("offer");

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

                foreach (XElement xOffer in xOffers)
                {
                    try
                    {
                        XElement xProduct = xOffer.Element("product");

                        XElement xCategory = xProduct.Element("category");
                        MatchProductCategory matchProductCategory = matchProductCategories.Where(mpc => mpc.SupplierProductCategoryName == xCategory.Value).FirstOrDefault();
                        if (matchProductCategory == null)
                        {
                            ProductCategory existingProductCategory = productCategories.Where(pc => pc.Name == xCategory.Value).FirstOrDefault();
                            matchProductCategory = new MatchProductCategory { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierProductCategoryName = xCategory.Value, ProductCategoryId = existingProductCategory == null ? (Guid?)null : existingProductCategory.Id };
                            db.MatchProductCategories.Add(matchProductCategory);
                            logger.Stats.NewProductCategoriesAdded++;
                        }

                        XElement xVolumeType = xProduct.Element("volumetype");
                        MatchVolumeType matchVolumeType = null;
                        if (xVolumeType != null)
                        {
                            matchVolumeType = matchVolumeTypes.Where(mvt => mvt.SupplierVolumeTypeName == xVolumeType.Value).FirstOrDefault();
                            if (matchVolumeType == null)
                            {
                                VolumeType existingVolumeType = volumeTypes.Where(vt => vt.Name == xVolumeType.Value).FirstOrDefault();
                                matchVolumeType = new MatchVolumeType { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierVolumeTypeName = xVolumeType.Value, VolumeTypeId = existingVolumeType == null ? (Guid?)null : existingVolumeType.Id };
                                db.MatchVolumeTypes.Add(matchVolumeType);
                                logger.Stats.NewVolumeTypesAdded++;
                            }
                        }

                        XElement xVolumeUnit = xProduct.Element("volumeunit");
                        MatchVolumeUnit matchVolumeUnit = null;
                        if (xVolumeUnit != null)
                        {
                            matchVolumeUnit = matchVolumeUnits.Where(mvu => mvu.SupplierVUShortName == xVolumeUnit.Attribute("shortname").Value && mvu.SupplierVUFullName == xVolumeUnit.Attribute("fullname").Value).FirstOrDefault();
                            if (matchVolumeUnit == null)
                            {
                                VolumeUnit existingVolumeUnit = volumeUnits.Where(vu => vu.ShortName == xVolumeUnit.Attribute("shortname").Value && vu.FullName == xVolumeUnit.Attribute("fullname").Value).FirstOrDefault();
                                matchVolumeUnit = new MatchVolumeUnit { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierVUShortName = xVolumeUnit.Attribute("shortname").Value, SupplierVUFullName = xVolumeUnit.Attribute("fullname").Value, VolumeUnitId = existingVolumeUnit == null ? (Guid?)null : existingVolumeUnit.Id };
                                db.MatchVolumeUnits.Add(matchVolumeUnit);
                                logger.Stats.NewVolumeUnitsAdded++;
                            }
                        }


                        XElement xQuantityUnit = xOffer.Element("quantityunit");
                        MatchQuantityUnit matchQuantityUnit = matchQuantityUnits.Where(mqu => mqu.SupplierQUShortName == xQuantityUnit.Attribute("shortname").Value && mqu.SupplierQUFullName == xQuantityUnit.Attribute("fullname").Value).FirstOrDefault();
                        if (matchQuantityUnit == null)
                        {
                            QuantityUnit existingQuantityUnit = quantityUnits.Where(qu => qu.ShortName == xQuantityUnit.Attribute("shortname").Value && qu.FullName == xQuantityUnit.Attribute("fullname").Value).FirstOrDefault();
                            matchQuantityUnit = new MatchQuantityUnit { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierQUShortName = xQuantityUnit.Attribute("shortname").Value, SupplierQUFullName = xQuantityUnit.Attribute("fullname").Value, QuantityUnitId = existingQuantityUnit == null ? (Guid?)null : existingQuantityUnit.Id };
                            db.MatchQuantityUnits.Add(matchQuantityUnit);
                            logger.Stats.NewQuantityUnitsAdded++;
                        }


                        List<MatchProductExtraProperty> productExtraProperties = new List<MatchProductExtraProperty>();

                        if (xProduct.Element("extraproperties") != null)
                        {
                            IEnumerable<XElement> xExtraProperties = xProduct.Element("extraproperties").Elements("extraproperty");
                            foreach (XElement xExtraProperty in xExtraProperties)
                            {
                                MatchProductExtraPropertyType matchProductExtraPropertyType = matchExtraPropertyTypes.Where(mept => mept.SupplierProductExtraPropertyTypeName == xExtraProperty.Attribute("type").Value).FirstOrDefault();
                                if (matchProductExtraPropertyType == null)
                                {
                                    ProductExtraPropertyType existingExtraPropertyType = extraPropertyTypes.Where(ept => ept.Name == xExtraProperty.Attribute("type").Value).FirstOrDefault();
                                    matchProductExtraPropertyType = new MatchProductExtraPropertyType { Id = Guid.NewGuid(), SupplierId = supplier.Id, SupplierProductExtraPropertyTypeName = xExtraProperty.Attribute("type").Value, ProductExtraPropertyTypeId = existingExtraPropertyType == null ? (Guid?)null : existingExtraPropertyType.Id };
                                    db.MatchProductExtraPropertyTypes.Add(matchProductExtraPropertyType);
                                    logger.Stats.NewExtraPropertiesAdded++;
                                }
                                productExtraProperties.Add(new MatchProductExtraProperty { Id = Guid.NewGuid(), MatchProductExtraPropertyTypeId = matchProductExtraPropertyType.Id, Value = xExtraProperty.Attribute("value").Value });
                            }

                            productExtraProperties = productExtraProperties.OrderBy(pep => pep.MatchProductExtraPropertyTypeId).ToList();
                        }
                        db.SaveChanges();

                        MatchOffer matchOfferExists = matchOffers.Where(of => of.SupplierProductCode == xOffer.Attribute("supplierproductcode").Value).FirstOrDefault();
                        bool addNewMatchOffer = false;

                        if (matchOfferExists == null)
                        {
                            addNewMatchOffer = true;
                        }
                        else
                        {
                            bool offerFullyMatches = true;
                            Guid MatchProductCategoryIdToCompare = matchProductCategories.Where(mpc => mpc.SupplierProductCategoryName == xProduct.Element("category").Value).Select(mpc => mpc.Id).FirstOrDefault();
                            Guid MatchQuantityUnitIdToCompare = matchQuantityUnits.Where(mqu => mqu.SupplierQUShortName == xQuantityUnit.Attribute("shortname").Value && mqu.SupplierQUFullName == xQuantityUnit.Attribute("fullname").Value).Select(mqu => mqu.Id).FirstOrDefault();
                            Guid MatchVolumeTypeIdToCompare = matchVolumeTypes.Where(mvt => mvt.SupplierVolumeTypeName == xVolumeType?.Value).Select(mvt => mvt.Id).FirstOrDefault();
                            Guid MatchVolumeUnitIdToCompare = matchVolumeUnits.Where(mvu => mvu.SupplierVUShortName == xVolumeUnit?.Attribute("shortname").Value && mvu.SupplierVUFullName == xVolumeUnit?.Attribute("fullname").Value).Select(mvu => mvu.Id).FirstOrDefault();
                            if (matchOfferExists.MatchProductCategoryId != MatchProductCategoryIdToCompare ||
                                matchOfferExists.MatchQuantityUnitId != MatchQuantityUnitIdToCompare ||
                                matchOfferExists.MatchVolumeTypeId != MatchVolumeTypeIdToCompare ||
                                matchOfferExists.MatchVolumeUnitId != MatchVolumeUnitIdToCompare ||
                                matchOfferExists.ProductName != xProduct.Element("name").Value ||
                                matchOfferExists.ProductVolume != (xProduct.Element("volume") == null? 0 : Convert.ToDecimal(xProduct.Element("volume").Value, new CultureInfo("en-US"))))
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
                                    correspondingOffer = db.Offers.Where(o => o.Id == matchOfferExists.OfferId).FirstOrDefault();
                                    correspondingOffer.Remains = Convert.ToDecimal(xOffer.Element("remains").Value, new CultureInfo("en-US"));
                                    correspondingOffer.RetailPrice = Convert.ToDecimal(xOffer.Element("retailprice").Value, new CultureInfo("en-US"));
                                    correspondingOffer.DiscountPrice = Convert.ToDecimal(xOffer.Element("discountprice").Value, new CultureInfo("en-US"));
                                    db.Offers.Update(correspondingOffer);
                                }
                                else
                                {
                                    matchOfferExists.Remains = Convert.ToDecimal(xOffer.Element("remains").Value, new CultureInfo("en-US"));
                                    matchOfferExists.RetailPrice = Convert.ToDecimal(xOffer.Element("retailprice").Value, new CultureInfo("en-US"));
                                    matchOfferExists.DiscountPrice = Convert.ToDecimal(xOffer.Element("discountprice").Value, new CultureInfo("en-US"));
                                    db.MatchOffers.Update(matchOfferExists);
                                }
                            }
                            else
                            {
                                Offer correspondingOffer;
                                if (matchOfferExists.OfferId != null)
                                {
                                    correspondingOffer = db.Offers.Where(o => o.Id == matchOfferExists.OfferId).FirstOrDefault();
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
                                SupplierProductCode = xOffer.Attribute("supplierproductcode").Value,
                                MatchProductCategoryId = matchProductCategory.Id,
                                MatchQuantityUnitId = matchQuantityUnit.Id,
                                MatchVolumeTypeId = matchVolumeType?.Id,
                                MatchVolumeUnitId = matchVolumeUnit?.Id,
                                ProductName = xProduct.Element("name").Value,
                                ProductVolume = (xProduct.Element("volume") == null ? 0 : Convert.ToDecimal(xProduct.Element("volume").Value, new CultureInfo("en-US"))),
                                Remains = Convert.ToDecimal(xOffer.Element("remains").Value, new CultureInfo("en-US")),
                                RetailPrice = Convert.ToDecimal(xOffer.Element("retailprice").Value, new CultureInfo("en-US")),
                                DiscountPrice = Convert.ToDecimal(xOffer.Element("discountprice").Value, new CultureInfo("en-US"))
                            };

                            db.MatchOffers.Add(newMatchOffer);
                            matchOfferExists = newMatchOffer;
                            logger.Stats.NewOffersAdded++;

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

                    catch (Exception e)
                    {
                        logger.Stats.Exceptions.Add(new UpdateException(e.Message, e) { CodeBlock = $"Offers Extraction - OfferInfo, SupplierProductCode:{xOffer.Attribute("SupplierProductCode").Value}", FilePath = xmlFile });
                        exceptionsFound = true;
                    }
                }
            }
            if (exceptionsFound == false)
            {
                logger.Stats.OffersExtractions.Last().IsSuccessful = true;
                try
                {
                    File.Delete(xmlFile);
                }
                catch (Exception e)
                {
                    logger.Stats.Exceptions.Add(new UpdateException(e.Message, e) { CodeBlock = "Offers Extraction - Cannot delete uploaded xmlFile", FilePath = xmlFile });
                    return;
                }
            }
            //Remove all info regarding offers that are in Db but doesn't exist in current extraction
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    List<Guid> allExistingMatchOffersIds = db.MatchOffers.Where(mo => mo.SupplierId == supplier.Id).Select(mo => mo.Id).ToList();
                    List<Guid> matchOffersToRemoveIds = allExistingMatchOffersIds.Except(offersProcessedIds).ToList();
                    foreach (Guid matchOfferToRemoveId in matchOffersToRemoveIds)
                    {
                        MatchOffer matchOfferToRemove = db.MatchOffers.Where(mo => mo.Id == matchOfferToRemoveId).FirstOrDefault();
                        if (matchOfferToRemove.OfferId != null)
                        {
                            List<CurrentOrder> curOrders = db.CurrentOrders.Include(co => co.Offer).Where(co => co.OfferId == matchOfferToRemove.OfferId).ToList();
                            if (curOrders.Count > 0)
                            {
                                foreach (CurrentOrder order in curOrders)
                                    order.Offer.Remains = 0;
                                db.CurrentOrders.UpdateRange(curOrders);
                                db.SaveChanges();
                            }
                            else
                            {
                                db.Offers.Remove(db.Offers.Where(o => o.Id == matchOfferToRemove.OfferId).FirstOrDefault());
                                db.SaveChanges();
                                logger.Stats.ExistingOffersDeleted++;
                            }
                        }

                        db.MatchProductExtraProperties.RemoveRange(db.MatchProductExtraProperties.Where(mpep => mpep.MatchOfferId == matchOfferToRemove.Id));
                        db.SaveChanges();
                        db.MatchOffers.Remove(matchOfferToRemove);
                        db.SaveChanges();
                        logger.Stats.MatchOffersDeleted++;
                    }
                }

            }
            catch (Exception e)
            {
                logger.Stats.Exceptions.Add(new UpdateException(e.Message, e) { CodeBlock = "Offers Extraction - Removing unused offers after successful uploading", FilePath = xmlFile });
                return;
            }
        }

        public static void ProcessPicturesXMLFromFile(string xmlFile, UpdateLogger logger)
        {
            XDocument xDoc = XDocument.Load(xmlFile);
            XElement xProductPictures = xDoc.Root;
            Guid supplierId = new Guid(xProductPictures.Attribute("supplierid").Value);
            IEnumerable<XElement> xPictures = xProductPictures.Elements("productpicture");


            foreach (XElement xPicture in xPictures)
            {
                string supplierProductCode = xPicture.Attribute("supplierproductcode").Value;
                string xmlFileDirectory = Path.GetDirectoryName(xmlFile);
                string supplierShortFilePath = xPicture.Element("picturefile").Value;
                string supplierPicFullFilePath = xmlFileDirectory + @"\" + supplierShortFilePath;
                try
                {
                    if (File.Exists(supplierPicFullFilePath))
                    {
                        MatchOffer matchOffer;
                        using (MarketDbContext db = new MarketDbContext())
                        {
                            matchOffer = db.MatchOffers.Where(mo => mo.SupplierId == supplierId && mo.SupplierProductCode == supplierProductCode).FirstOrDefault();
                        }
                        if (matchOffer != null)
                        {
                            if (matchOffer.OfferId == null)
                            {
                                //Adding new Unmatched Picture
                                Guid newUnmatchedPicGuid = Guid.NewGuid();
                                using (MarketDbContext db = new MarketDbContext())
                                {
                                    if (db.UnmatchedPics.Where(up => up.SupplierId == supplierId && up.SupplierProductCode == supplierProductCode).FirstOrDefault() == null)
                                    {
                                        string destFileName = CoreSettings.b2bDataLocalDir + CoreSettings.UnmatchedProductsPicturesPath + "/" + newUnmatchedPicGuid.ToString() + CoreSettings.PictureExtension;
                                        if (ImageProcessor.ResizeAndConvertProductImageToJpeg(new Uri(supplierPicFullFilePath), new Uri(destFileName), CoreSettings.ProductPictureWidth, CoreSettings.ProductPictureHeight))
                                        {
                                            db.UnmatchedPics.Add(new UnmatchedPic { Id = newUnmatchedPicGuid, SupplierId = supplierId, SupplierProductCode = supplierProductCode });
                                            logger.Stats.NewUnmatchedPicsAdded++;
                                        }
                                        else
                                        {
                                            logger.Stats.ProblemPics++;
                                        }
                                    }
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                Guid productId;
                                using (MarketDbContext db = new MarketDbContext())
                                {
                                    productId = db.Offers.Where(o => o.Id == matchOffer.OfferId).Select(o => o.ProductId).FirstOrDefault();
                                }
                                string destFileName = CoreSettings.b2bDataLocalDir + CoreSettings.MatchedProductsPicturesPath + "/" + productId.ToString() + CoreSettings.PictureExtension;

                                if (File.Exists(destFileName))
                                {
                                    //Adding New Conflicted
                                    Guid newConflictedPicGuid = Guid.NewGuid();

                                    using (MarketDbContext db = new MarketDbContext())
                                    {
                                        if (db.ConflictedPics.Where(up => up.SupplierId == supplierId && up.ProductId == productId).FirstOrDefault() == null)
                                        {
                                            destFileName = CoreSettings.b2bDataLocalDir + CoreSettings.ConflictedProductsPicturesPath + "/" + newConflictedPicGuid.ToString() + CoreSettings.PictureExtension;
                                            if (ImageProcessor.ResizeAndConvertProductImageToJpeg(new Uri(supplierPicFullFilePath), new Uri(destFileName), CoreSettings.ProductPictureWidth, CoreSettings.ProductPictureHeight))
                                            {
                                                db.ConflictedPics.Add(new ConflictedPic { Id = newConflictedPicGuid, SupplierId = supplierId, ProductId = productId });
                                                db.SaveChanges();
                                                logger.Stats.NewConflictedPicsAdded++;
                                            }
                                            else
                                            {
                                                logger.Stats.ProblemPics++;
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    //Adding new Matched Picture
                                    if (ImageProcessor.ResizeAndConvertProductImageToJpeg(new Uri(supplierPicFullFilePath), new Uri(destFileName), CoreSettings.ProductPictureWidth, CoreSettings.ProductPictureHeight))
                                    {
                                        logger.Stats.NewMatchedPicsAdded++;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Stats.Exceptions.Add(new UpdateException(e.Message, e) { CodeBlock = $"Pictures Extraction - SupplierProductCode:{supplierProductCode}, PictureFilePath:\"{supplierPicFullFilePath}\"", FilePath = xmlFile });
                }
            }
            logger.Stats.PicturesExtractions.Last().IsSuccessful = true;
            File.Delete(xmlFile);
        }

        public static void ProcessDescriptionsXMLFromFile(string xmlFile, UpdateLogger logger)
        {
            XDocument xDoc = XDocument.Load(xmlFile);
            XElement xProductDescriptions = xDoc.Root;
            Guid supplierId = new Guid(xProductDescriptions.Attribute("supplierid").Value);
            IEnumerable<XElement> xDescriptions = xProductDescriptions.Elements("productdescription");


            foreach (XElement xDescription in xDescriptions)
            {
                string supplierProductCode = xDescription.Attribute("supplierproductcode").Value;
                string xmlFileDirectory = Path.GetDirectoryName(xmlFile);
                string supplierDescription = xDescription.Element("description").Value;

                try
                {
                    MatchOffer matchOffer;
                    using (MarketDbContext db = new MarketDbContext())
                    {
                        matchOffer = db.MatchOffers.Where(mo => mo.SupplierId == supplierId && mo.SupplierProductCode == supplierProductCode).FirstOrDefault();
                    }
                    if (matchOffer != null)
                    {
                        if (matchOffer.OfferId == null)
                        {
                            Guid newUnmatchedPicGuid = Guid.NewGuid();
                            using (MarketDbContext db = new MarketDbContext())
                            {
                                if (db.UnmatchedDescriptions.Where(ud => ud.SupplierId == supplierId && ud.SupplierProductCode == supplierProductCode).FirstOrDefault() == null)
                                {
                                    db.UnmatchedDescriptions.Add(new UnmatchedDescription { Id = Guid.NewGuid(), SupplierId = supplierId, SupplierProductCode = supplierProductCode, Description = supplierDescription });
                                    logger.Stats.NewUnmatchedDescsAdded++;
                                }
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            Guid productId;
                            using (MarketDbContext db = new MarketDbContext())
                            {
                                productId = db.Offers.Where(o => o.Id == matchOffer.OfferId).Select(o => o.ProductId).FirstOrDefault();
                            }
                            ProductDescription prodDesc;
                            using (MarketDbContext db = new MarketDbContext())
                            {
                                prodDesc = db.ProductDescriptions.Where(pd => pd.ProductId == productId).FirstOrDefault();
                            }

                            if (prodDesc == null || prodDesc.Text == "")
                            {
                                using (MarketDbContext db = new MarketDbContext())
                                {

                                    if (prodDesc == null)
                                    {
                                        db.ProductDescriptions.Add(new ProductDescription { ProductId = productId, Text = supplierDescription });
                                    }
                                    else
                                    {
                                        prodDesc.Text = supplierDescription;
                                        db.ProductDescriptions.Update(prodDesc);
                                    }
                                    db.SaveChanges();
                                    logger.Stats.NewMatchedDescsAdded++;

                                }
                            }
                            else
                            {
                                if (prodDesc.Text != supplierDescription)
                                {
                                    using (MarketDbContext db = new MarketDbContext())
                                    {
                                        if (db.ConflictedDescriptions.Where(up => up.SupplierId == supplierId && up.ProductId == productId).FirstOrDefault() == null)
                                        {
                                            db.ConflictedDescriptions.Add(new ConflictedDescription { Id = Guid.NewGuid(), SupplierId = supplierId, ProductId = productId, Description = supplierDescription });
                                            db.SaveChanges();
                                            logger.Stats.NewConflictedDescsAdded++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Stats.Exceptions.Add(new UpdateException(e.Message, e) { CodeBlock = $"Descriptions Extraction - SupplierProductCode:{supplierProductCode}", FilePath = xmlFile });
                }
            }
            logger.Stats.DescriptionsExtractions.Last().IsSuccessful = true;
            File.Delete(xmlFile);
        }
    }
}
