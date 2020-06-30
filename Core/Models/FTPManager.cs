using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Models
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
                catch (WebException e)
                {
                    return null;
                }
            }

        }

        public static byte[] GetProductPicture(Guid productGuid)
        {
            return GetPictureFromFTP(AdminFTPAccessString + "/ProductPics", productGuid.ToString() + ".png");
        }

        public static byte[] GetTopCategoryPicture(Guid productGuid)
        {
            return GetPictureFromFTP(AdminFTPAccessString + "/TopCategoryPics", productGuid.ToString() + ".png");
        }

        public static byte[] GetSupplierPicture(string FTPAccessString)
        {
            return GetPictureFromFTP(FTPAccessString, "Logo.png");
        }
    }
}
