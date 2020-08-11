using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.DBModels;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UpdateDb_Service
{
    public class Program
    {
       
        public static void CheckAndUpdateSuppliersOffers()
        {
            StreamWriter logFileStream = new StreamWriter(CoreSettings.b2bDataLocalDir + "/" + CoreSettings.LogFileName, true, System.Text.Encoding.UTF8);
            logFileStream.WriteLine("****************** - " + DateTime.Now.ToString("G") + " - Process of checking for new extractions from suppliers started ******************");

            List<string> suppliersDirectoriesList;
            suppliersDirectoriesList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/").GetDirectories().Select(f => f.Name).ToList();

            foreach (string supplierDirectory in suppliersDirectoriesList)
            {
                List<string> ExtractionsFileList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierOffersPath).GetFiles().Select(f => f.Name).OrderBy(s => s).ToList();
                if (ExtractionsFileList.Count > 0)
                    logFileStream.WriteLine(DateTime.Now.ToString("G") + " - Found " + ExtractionsFileList.Count.ToString() + " new extractions of supplier: " + supplierDirectory);

                foreach (string ExtractionFileName in ExtractionsFileList)
                {
                    logFileStream.WriteLine(DateTime.Now.ToString("G") + " - Starting to process extraction " + supplierDirectory + ": " + ExtractionFileName);
                    XMLProcessor.ProcessOffersXMLFromFile(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierOffersPath + "/" + ExtractionFileName, logFileStream);
                }
            }

            logFileStream.WriteLine("****************** - " + DateTime.Now.ToString("G") + " - Process of checking for new extractions from suppliers finished ******************");
            logFileStream.Close();
        }

        public static void CheckAndUpdatePics()
        {
            StreamWriter logFileStream = new StreamWriter(CoreSettings.b2bDataLocalDir + "/" + CoreSettings.LogFileName, true, System.Text.Encoding.UTF8);
            logFileStream.WriteLine("****************** - " + DateTime.Now.ToString("G") + " - Process of checking for new product pictures from suppliers started ******************");

            List<string> suppliersDirectoriesList;
            suppliersDirectoriesList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/").GetDirectories().Select(f => f.Name).ToList();

            foreach (string supplierDirectory in suppliersDirectoriesList)
            {
                List<string> DescriptionsFileList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierDescriptionsPath).GetFiles().Select(f => f.Name).OrderBy(s => s).ToList();

                foreach (string DescriptionFileName in DescriptionsFileList)
                {
                    if (DescriptionFileName == CoreSettings.ProductPicturesExtractionFileName)
                    {
                        logFileStream.WriteLine(DateTime.Now.ToString("G") + " - Found " + DescriptionsFileList.Count.ToString() + " new product pictures of supplier: " + supplierDirectory);
                        XMLProcessor.ProcessPicturesXMLFromFile(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierDescriptionsPath + "/" + DescriptionFileName, logFileStream);
                    }
                }
            }

            logFileStream.WriteLine("****************** - " + DateTime.Now.ToString("G") + " - Process of checking for new product pictures from suppliers finished ******************");
            logFileStream.Close();
        }

        public static void CheckAndUpdateDesc()
        {
            StreamWriter logFileStream = new StreamWriter(CoreSettings.b2bDataLocalDir + "/" + CoreSettings.LogFileName, true, System.Text.Encoding.UTF8);
            logFileStream.WriteLine("****************** - " + DateTime.Now.ToString("G") + " - Process of checking for new product descriptions from suppliers started ******************");

            List<string> suppliersDirectoriesList;
            suppliersDirectoriesList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/").GetDirectories().Select(f => f.Name).ToList();

            foreach (string supplierDirectory in suppliersDirectoriesList)
            {
                List<string> DescriptionsFileList = new DirectoryInfo(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierDescriptionsPath).GetFiles().Select(f => f.Name).OrderBy(s => s).ToList();

                foreach (string DescriptionFileName in DescriptionsFileList)
                {
                    if (DescriptionFileName == CoreSettings.ProductDescriptionsExtractionFileName)
                    {
                        logFileStream.WriteLine(DateTime.Now.ToString("G") + " - Found " + DescriptionsFileList.Count.ToString() + " new product descriptions of supplier: " + supplierDirectory);
                        XMLProcessor.ProcessDescriptionsXMLFromFile(CoreSettings.b2bDataLocalDir + CoreSettings.SuppliersPath + "/" + supplierDirectory + CoreSettings.SupplierDescriptionsPath + "/" + DescriptionFileName, logFileStream);
                    }
                }
            }

            logFileStream.WriteLine("****************** - " + DateTime.Now.ToString("G") + " - Process of checking for new product descriptions from suppliers finished ******************");
            logFileStream.WriteLine();
            logFileStream.Close();
        }

        public static void RemoveUnusedOffers()
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
                }
            }
            catch
            {
                return;
            }
        }

        public static void Main()
        {
            // CreateHostBuilder(args).Build().Run();
            CheckAndUpdateSuppliersOffers();
            CheckAndUpdatePics();
            CheckAndUpdateDesc();
            RemoveUnusedOffers();
        }

/*        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });*/
    }
}
