using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Core.Models
{
    public static class FTPManager
    {
        private static readonly string AdminFTPAccessString = "ftp://B2BAdmin:B2BAdminPassword@192.168.1.1";

        public static byte[] GetProductPicture(Guid productGuid)
        {
            using (WebClient wc = new WebClient())
                return wc.DownloadData(AdminFTPAccessString + "/ProductPics/" + productGuid.ToString() + ".png");
        }
    }
}
