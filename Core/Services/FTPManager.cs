using Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Services
{
    public static class FTPManager
    {
        private static readonly string AdminFTPAccessString = "ftp://B2BAdmin:B2BAdminPassword@192.168.1.1";
        private static readonly string b2bDataDir = @"\\192.168.1.1\Media Server\B2B FTP Server";


        private static byte[] GetPictureFromFTP(string FTPAccessDir, string FileName)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    return wc.DownloadData(FTPAccessDir + "/" + FileName);
                }
                catch (WebException)
                {
                    return null;
                }
            }
        }

        public static byte[] GetProductPicture(Guid productGuid)
        {
            return GetPictureFromFTP(AdminFTPAccessString + "/ProductPics", productGuid.ToString() + ".png");

            //TODO: * Direct disk access instead of ftp - Just for tests * - DELETE
            //return File.ReadAllBytes("C:/Working Projects/B2B_Market/B2B FTP Server dummy/ProductPics/" + productGuid.ToString() + ".png");
        }

        public static byte[] GetTopCategoryPicture(Guid productGuid)
        {
            return GetPictureFromFTP(AdminFTPAccessString + "/TopCategoryPics", productGuid.ToString() + ".png");

            //TODO: * Direct disk access instead of ftp - Just for tests * - DELETE
            //return File.ReadAllBytes("C:/Working Projects/B2B_Market/B2B FTP Server dummy/TopCategoryPics/" + productGuid.ToString() + ".png");
        }

        public static byte[] GetSupplierPicture(string FTPAccessString)
        {
            return GetPictureFromFTP(FTPAccessString, "Logo.png");

            //TODO: * Direct disk access instead of ftp - Just for tests * - DELETE
            /*string path = "C:/Working Projects/B2B_Market/B2B FTP Server dummy/Suppliers/";
            
            switch(FTPAccessString)
            {
                case "ftp://Adal:zxcvb@192.168.1.1":
                    path += "Adal";
                    break;
                case "ftp://Supplier4:asdfg@192.168.1.1":
                    path += "Supplier4";
                    break;
                case "ftp://Supplier3:qwerty@192.168.1.1":
                    path += "Supplier3";
                    break;
                case "ftp://Supplier2:Test@192.168.1.1":
                    path += "Supplier2";
                    break;
                case "ftp://FoodMaster:12345@192.168.1.1":
                    path += "FoodMaster";
                    break;
                case "ftp://Supplier1:Test@192.168.1.1":
                    path += "Supplier1";
                    break;
            }
            
            return File.ReadAllBytes(path + "/Logo.png");*/
        }

        public static bool UploadRequestToSupplierFTP(string FTPAccessString, ArchivedRequest request)
        {
            FtpWebRequest ftpRequest;
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create(FTPAccessString + "/Orders/Order" + request.Code.ToString() + ".xml");
            }
            catch
            {
                return false;
            }

            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            try
            {
                using (Stream ftpStream = ftpRequest.GetRequestStream())
                {
                    XMLProcessor.SaveRequestXMLToStream(request, ftpStream);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void CheckAndUpdateSuppliersOffers()
        {
            StreamWriter logFileStream = new StreamWriter(b2bDataDir + @"\AgentLog.txt", true, System.Text.Encoding.UTF8);
            logFileStream.WriteLine("****************** - " + DateTime.Now.ToString("G") + " - Process of checking for new extractions from suppliers started ******************");

            List<string> suppliersDirectoriesList;
            suppliersDirectoriesList = new DirectoryInfo(b2bDataDir + "\\Suppliers\\").GetDirectories().Select(f => f.Name).ToList();
            logFileStream.WriteLine(DateTime.Now.ToString("G") + " - Found " + suppliersDirectoriesList.Count.ToString() + " supplier directories");

            foreach (string supplierDirectory in suppliersDirectoriesList)
            {
                List<string> ExtractionsFileList = new DirectoryInfo(b2bDataDir + @"\Suppliers\" + supplierDirectory + @"\Offers").GetFiles().Select(f => f.Name).OrderBy(s => s).ToList(); //TODO Replace with foreach for every supplier
                logFileStream.WriteLine(DateTime.Now.ToString("G") + " - Found " + ExtractionsFileList.Count.ToString() + " new extractions of supplier: " + supplierDirectory);


                foreach (string ExtractionFileName in ExtractionsFileList)
                {
                    logFileStream.WriteLine(DateTime.Now.ToString("G") + " - Starting to process extraction " + supplierDirectory + ": " + ExtractionFileName);
                    XMLProcessor.ProcessOffersXMLFromFile(b2bDataDir + @"\Suppliers\" + supplierDirectory + @"\Offers\" + ExtractionFileName, logFileStream);
                }
            }

            logFileStream.WriteLine("****************** - " + DateTime.Now.ToString("G") + " - Process of checking for new extractions from suppliers finished ******************");
            logFileStream.WriteLine();
            logFileStream.Close();
        }

    }
}
