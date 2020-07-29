using Core.DBModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Services
{
    public static class FTPManager
    {
        // private static readonly string AdminFTPAccessString = "ftp://B2BAdmin:B2BAdminPassword@192.168.1.1"; //RemoteFTP
         private static readonly string AdminFTPAccessString = "ftp://B2BAdmin:B2BAdminPassword@localhost"; //LocalFTP


        private static bool FileExists(string AccessStringPath, string fileName)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(AccessStringPath);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            using (Stream respStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(respStream);
                //Read each file name from the response
                for (string fname = reader.ReadLine(); fname != null; fname = reader.ReadLine())
                {
                    if (fname == fileName) return true;
                }
            }
            return false;
        }

        public static bool? MoveUnmatchedProductPicToMatched(Guid unmatchedPicId, Guid matchedProductId)
        {

            Uri uriSource = new Uri(AdminFTPAccessString + "/Pictures/Products/Unmatched/" + unmatchedPicId.ToString()+".png");

            if (FileExists(AdminFTPAccessString + "/Pictures/Products/Matched/", matchedProductId.ToString() + ".png"))
                return false;

            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.Rename;
            reqFTP.RenameTo = "../Matched/" + matchedProductId.ToString() + ".png";

            try
            {
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            }
            catch (WebException)
            {
                return null;
            }

            return true;
        }

        public static bool AreUmatchedAndMatchedPicsTheSame(Guid unmatchedPicId, Guid productId)
        {
            Uri uriUnmatched = new Uri(AdminFTPAccessString + "/Pictures/Products/Unmatched/" + unmatchedPicId.ToString() + ".png");
            Uri uriMatched = new Uri(AdminFTPAccessString + "/Pictures/Products/Matched/" + productId.ToString() + ".png");
            byte[] unmatchedPicData;
            byte[] matchedPicData;
            using (WebClient wc = new WebClient())
            {
                unmatchedPicData = wc.DownloadData(uriUnmatched);
                matchedPicData = wc.DownloadData(uriMatched);
            }
            if (unmatchedPicData.SequenceEqual(matchedPicData))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void RemoveUnmatchedPic(Guid unmatchedPicId)
        {
            Uri uriSource = new Uri(AdminFTPAccessString + "/Pictures/Products/Unmatched/" + unmatchedPicId.ToString() + ".png");
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            try
            {
                reqFTP.GetResponse();
            }
            catch
            {
                return;
            }
            return;
        }

        public static void RemoveMatchedPic(Guid productId)
        {
            Uri uriSource = new Uri(AdminFTPAccessString + "/Pictures/Products/Matched/" + productId.ToString() + ".png");
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            try
            {
                reqFTP.GetResponse();
            }
            catch
            {
                return;
            }
        }

        public static void RemoveConflictedPic(Guid conflictedPicId)
        {
            Uri uriSource = new Uri(AdminFTPAccessString + "/Pictures/Products/Conflicted/" + conflictedPicId.ToString() + ".png");
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            try
            {
                reqFTP.GetResponse();
            }
            catch
            {
                return;
            }
            return;
        }

        public static bool? MoveUnmatchedProductPicToConflicted(Guid unmatchedPicId, Guid conflictedPicId)
        {

            Uri uriSource = new Uri(AdminFTPAccessString + "/Pictures/Products/Unmatched/" + unmatchedPicId.ToString() + ".png");

            if (FileExists(AdminFTPAccessString + "/Pictures/Products/Conflicted", conflictedPicId.ToString() + ".png"))
                return null;

            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.Rename;
            reqFTP.RenameTo = "../Conflicted/" + conflictedPicId.ToString() + ".png";

            try
            {
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            }
            catch (WebException)
            {
                return null;
            }

            return true;
        }

        public static byte[] GetConflictedPicture(Guid guid)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    return wc.DownloadData(AdminFTPAccessString+"/Pictures/Products/Conflicted/"+guid.ToString()+".png");
                }
            }
            catch(WebException)
            {
                return null;
            }
        }

        public static void UpdateMatchedPicWithConflicted(Guid conflictedPicId, Guid productId)
        {
            RemoveMatchedPic(productId);

            Uri uriSource = new Uri(AdminFTPAccessString + "/Pictures/Products/Conflicted/" + conflictedPicId.ToString() + ".png");
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.Rename;
            reqFTP.RenameTo = "../Matched/" + productId.ToString() + ".png";

            try
            {
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            }
            catch (WebException)
            {
                return;
            }

            return;
        }

        public static void UploadPicDataToMatchedPics(Guid productId, byte[] picData)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.UploadData(AdminFTPAccessString + "/Pictures/Products/Matched/" + productId.ToString() + ".png", picData);
                }
                catch
                {
                    return;
                }
            }
        }

        private static void DeleteReqProdPicsFile(string FTPAccessString)
        {
            Uri uriSource = new Uri(FTPAccessString + "/Descriptions/ReqProdPics.xml");
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            try
            {
                reqFTP.GetResponse();
            }
            catch
            {
                return;
            }
        }

        private static void DeleteReqProdDescFile(string FTPAccessString)
        {
            Uri uriSource = new Uri(FTPAccessString + "/Descriptions/ReqProdDesc.xml");
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            try
            {
                reqFTP.GetResponse();
            }
            catch
            {
                return;
            }
        }

        public static bool UpdateReqProdPicsFile(MemoryStream stream, string FTPAccessString)
        {
            DeleteReqProdPicsFile(FTPAccessString);
            using (WebClient wc = new WebClient())
            {
                try
                {
                    Stream streamToWrite = wc.OpenWrite(FTPAccessString + "/Descriptions/ReqProdPics.xml");
                    stream.WriteTo(streamToWrite);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static bool UpdateReqProdDescFile(MemoryStream stream, string FTPAccessString)
        {
            DeleteReqProdDescFile(FTPAccessString);
            using (WebClient wc = new WebClient())
            {
                try
                {
                    Stream streamToWrite = wc.OpenWrite(FTPAccessString + "/Descriptions/ReqProdDesc.xml");
                    stream.WriteTo(streamToWrite);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static Stream GetReqProdPicsStreamIfAvailable(string FTPAccessString)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    return wc.OpenRead(FTPAccessString + "/Descriptions/ReqProdPics.xml");
                }
                catch
                {
                    return null;
                }
            }
        }

        public static Stream GetReqProdDescStreamIfAvailable(string FTPAccessString)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    return wc.OpenRead(FTPAccessString + "/Descriptions/ReqProdDesc.xml");
                }
                catch
                {
                    return null;
                }
            }
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
    }
}
