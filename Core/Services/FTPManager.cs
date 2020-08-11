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
        public static string GetFTPAccessString(string user, string password)
        {
            return "ftp://" + user + ":" + password + "@" + CoreSettings.ServerIP;
        }

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

        public static void DeleteFile(Uri uri)
        {
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uri);
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

        public static List<Guid> GetProductsMatchedPicturesGuids()
        {
            List<Guid> result = new List<Guid>();
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(CoreSettings.FTPAdminAccessString+CoreSettings.MatchedProductsPicturesPath+"/");
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            using (Stream respStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(respStream);
                //Read each file name from the response
                for (string fname = reader.ReadLine(); fname != null; fname = reader.ReadLine())
                {
                    result.Add(new Guid(Path.GetFileNameWithoutExtension(fname)));
                }
            }
            return result;

        }

        public static bool? MoveUnmatchedProductPicToMatched(Guid unmatchedPicId, Guid matchedProductId)
        {

            Uri uriSource = new Uri(CoreSettings.FTPAdminAccessString + CoreSettings.UnmatchedProductsPicturesPath + "/"+ unmatchedPicId.ToString()+CoreSettings.PictureExtension);

            if (FileExists(CoreSettings.FTPAdminAccessString + CoreSettings.MatchedProductsPicturesPath+"/", matchedProductId.ToString() + CoreSettings.PictureExtension))
                return false;

            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.Rename;
            reqFTP.RenameTo = ".." + CoreSettings.MatchedProductsPicturesDir + "/" + matchedProductId.ToString() + CoreSettings.PictureExtension;

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
            Uri uriUnmatched = new Uri(CoreSettings.FTPAdminAccessString + CoreSettings.UnmatchedProductsPicturesPath + "/" + unmatchedPicId.ToString() + CoreSettings.PictureExtension);
            Uri uriMatched = new Uri(CoreSettings.FTPAdminAccessString + CoreSettings.MatchedProductsPicturesPath + "/" + productId.ToString() + CoreSettings.PictureExtension);
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
            Uri uriSource = new Uri(CoreSettings.FTPAdminAccessString + CoreSettings.UnmatchedProductsPicturesPath + "/" + unmatchedPicId.ToString() + CoreSettings.PictureExtension);
            DeleteFile(uriSource);
        }

        public static void RemoveMatchedPic(Guid productId)
        {
            Uri uriSource = new Uri(CoreSettings.FTPAdminAccessString + CoreSettings.MatchedProductsPicturesPath + "/" + productId.ToString() + CoreSettings.PictureExtension);
            DeleteFile(uriSource);
        }

        public static void RemoveConflictedPic(Guid conflictedPicId)
        {
            Uri uriSource = new Uri(CoreSettings.FTPAdminAccessString + CoreSettings.ConflictedProductsPicturesPath + "/" + conflictedPicId.ToString() + CoreSettings.PictureExtension);
            DeleteFile(uriSource);
        }

        public static bool? MoveUnmatchedProductPicToConflicted(Guid unmatchedPicId, Guid conflictedPicId)
        {

            Uri uriSource = new Uri(CoreSettings.FTPAdminAccessString + CoreSettings.UnmatchedProductsPicturesPath + "/" + unmatchedPicId.ToString() + CoreSettings.PictureExtension);

            if (FileExists(CoreSettings.FTPAdminAccessString + CoreSettings.ConflictedProductsPicturesPath + "/", conflictedPicId.ToString() + CoreSettings.PictureExtension))
                return null;

            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.Rename;
            reqFTP.RenameTo = ".." + CoreSettings.ConflictedProductsPictureDir+"/" + conflictedPicId.ToString() + CoreSettings.PictureExtension;

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
                    return wc.DownloadData(CoreSettings.FTPAdminAccessString + CoreSettings.ConflictedProductsPicturesPath + "/" + guid.ToString()+ CoreSettings.PictureExtension);
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

            Uri uriSource = new Uri(CoreSettings.FTPAdminAccessString + CoreSettings.ConflictedProductsPicturesPath + "/" + conflictedPicId.ToString() + CoreSettings.PictureExtension);
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(uriSource);
            reqFTP.Method = WebRequestMethods.Ftp.Rename;
            reqFTP.RenameTo = ".." + CoreSettings.MatchedProductsPicturesDir + "/" + productId.ToString() + CoreSettings.PictureExtension;

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
                    wc.UploadData(CoreSettings.FTPAdminAccessString + CoreSettings.MatchedProductsPicturesPath + "/" + productId.ToString() + CoreSettings.PictureExtension, picData);
                }
                catch
                {
                    return;
                }
            }
        }

        private static void DeleteFile(string FTPFileUri)
        {
            FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(FTPFileUri);
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

        public static bool UpdateReqProdPicsFile(MemoryStream stream, string FTPUser, string FTPPassword)
        {
            string FTPFileUri = GetFTPAccessString(FTPUser, FTPPassword)+CoreSettings.SupplierDescriptionsPath+"/"+CoreSettings.ProductPicturesRequestFileName;
            DeleteFile(FTPFileUri);
            using (WebClient wc = new WebClient())
            {
                try
                {
                    Stream streamToWrite = wc.OpenWrite(FTPFileUri);
                    stream.WriteTo(streamToWrite);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static bool UpdateReqProdDescFile(MemoryStream stream, string FTPUser, string FTPPassword)
        {
            string FTPFileUri = GetFTPAccessString(FTPUser, FTPPassword) + CoreSettings.SupplierDescriptionsPath + "/" + CoreSettings.ProductDescriptionsRequestFileName;
            DeleteFile(FTPFileUri);
            using (WebClient wc = new WebClient())
            {
                try
                {
                    Stream streamToWrite = wc.OpenWrite(FTPFileUri);
                    stream.WriteTo(streamToWrite);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static Stream GetReqProdPicsStreamIfAvailable(string FTPUser, string FTPPassword)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    return wc.OpenRead(GetFTPAccessString(FTPUser, FTPPassword) + CoreSettings.SupplierDescriptionsPath + "/" + CoreSettings.ProductPicturesRequestFileName);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static Stream GetReqProdDescStreamIfAvailable(string FTPUser, string FTPPassword)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    return wc.OpenRead(GetFTPAccessString(FTPUser, FTPPassword) + CoreSettings.SupplierDescriptionsPath + "/" + CoreSettings.ProductDescriptionsRequestFileName);
                }
                catch
                {
                    return null;
                }
            }
        }


        public static bool UploadRequestToSupplierFTP(string FTPUser, string FTPPassword, ArchivedRequest request)
        {
            FtpWebRequest ftpRequest;
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create(GetFTPAccessString(FTPUser, FTPPassword) + CoreSettings.SupplierOrdersPath + "/Order" + request.Code.ToString() + ".xml");
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
