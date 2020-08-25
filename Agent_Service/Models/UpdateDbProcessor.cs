using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Core.DBModels;
using Core.Models;
using Core.Services;
using Microsoft.EntityFrameworkCore;

namespace UpdateDb_Service.Models
{
    public static class UpdateDbProcessor
    {
        private static void CheckAndUpdateSuppliersOffers(UpdateLogger logger)
        {
            List<string> suppliersDirectoriesList;
            suppliersDirectoriesList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/").GetDirectories().Select(f => f.Name).ToList();

            foreach (string supplierDirectory in suppliersDirectoriesList)
            {
                List<string> ExtractionsFileList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierOffersPath).GetFiles().Select(f => f.Name).OrderBy(s => s).ToList();
                foreach (string ExtractionFileName in ExtractionsFileList)
                {
                    logger.Stats.OffersExtractions.Add(new ExtractionInfo(supplierDirectory, ExtractionFileName));
                    XMLProcessor.ProcessOffersXMLFromFile(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierOffersPath + "/" + ExtractionFileName, logger);
                }
            }
        }

        private static void CheckAndUpdatePics(UpdateLogger logger)
        {
            List<string> suppliersDirectoriesList;
            suppliersDirectoriesList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/").GetDirectories().Select(f => f.Name).ToList();

            foreach (string supplierDirectory in suppliersDirectoriesList)
            {
                List<string> ProductPicturesFileList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierDescriptionsPath).GetFiles().Select(f => f.Name).OrderBy(s => s).ToList();
                foreach (string PicturesFileName in ProductPicturesFileList)
                {
                    if (PicturesFileName == CoreSettings.ProductPicturesExtractionFileName)
                    {
                        logger.Stats.PicturesExtractions.Add(new ExtractionInfo(supplierDirectory, PicturesFileName));
                        XMLProcessor.ProcessPicturesXMLFromFile(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierDescriptionsPath + "/" + PicturesFileName, logger);
                    }
                }
            }
        }

        private static void CheckAndUpdateDesc(UpdateLogger logger)
        {
            List<string> suppliersDirectoriesList;
            suppliersDirectoriesList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/").GetDirectories().Select(f => f.Name).ToList();

            foreach (string supplierDirectory in suppliersDirectoriesList)
            {
                List<string> DescriptionsFileList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierDescriptionsPath).GetFiles().Select(f => f.Name).OrderBy(s => s).ToList();
                foreach (string DescriptionFileName in DescriptionsFileList)
                {
                    if (DescriptionFileName == CoreSettings.ProductDescriptionsExtractionFileName)
                    {
                        logger.Stats.DescriptionsExtractions.Add(new ExtractionInfo(supplierDirectory, DescriptionFileName));
                        XMLProcessor.ProcessDescriptionsXMLFromFile(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierDescriptionsPath + "/" + DescriptionFileName, logger);
                    }
                }
            }
        }

        private static void RemoveUnusedOffers(UpdateLogger logger)
        {
            try
            {
                using (MarketDbContext db = new MarketDbContext())
                {
                    List<Offer> noRemainsOffers = db.Offers.Where(o => o.Remains == 0).ToList();
                    List<Offer> noRemainsOffersWithExistingMatchedOffers = db.MatchOffers.Include(mo => mo.Offer).Where(mo => mo.Offer.Remains == 0).Select(mo => mo.Offer).ToList();
                    noRemainsOffers = noRemainsOffers.Except(noRemainsOffersWithExistingMatchedOffers).ToList();
                    List<Offer> noRemainsOffersInExistingOrders = db.CurrentOrders.Include(co => co.Offer).Where(co => co.Offer.Remains == 0).Select(co => co.Offer).ToList();
                    db.Offers.RemoveRange(noRemainsOffers.Except(noRemainsOffersInExistingOrders));
                    db.SaveChanges();
                    logger.Stats.ExistingOffersDeleted += noRemainsOffers.Except(noRemainsOffersInExistingOrders).Count();
                }
            }
            catch(Exception e)
            {
                logger.Stats.Exceptions.Add(new UpdateException(e.Message, e) { CodeBlock = "Remove NoRemainsOffers", FilePath = "" });
                return;
            }
        }

        public static void UpdateDb()
        {
            UpdateLogger logger = new UpdateLogger(new Uri(CoreSettings.b2bDataLocalDir + "/" + CoreSettings.LogFileName));

            CheckAndUpdateSuppliersOffers(logger);
            CheckAndUpdatePics(logger);
            CheckAndUpdateDesc(logger);
            RemoveUnusedOffers(logger);
            
            logger.ProcessUpdateStats();

            logger.Dispose();
        }
    }
}
