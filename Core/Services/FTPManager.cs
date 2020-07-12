using Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Services
{
    public static class FTPManager
    {
        private static readonly string AdminFTPAccessString = "ftp://B2BAdmin:B2BAdminPassword@192.168.1.1";

        private static byte[] GetPictureFromFTP(string FTPAccessDir, string FileName)
        {
             using (WebClient wc = new WebClient())
             {
                 try 
                 {
                     return wc.DownloadData(FTPAccessDir + "/" + FileName);
                 }
                 catch(WebException)                  
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
                    XMLProcessor.SaveRequestXML(request, ftpStream);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
