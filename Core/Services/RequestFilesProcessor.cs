using System;
using System.IO;
using Core.Models;

namespace Core.Services
{
    public static class RequestFilesProcessor
    {
        public static bool CreateNewRequestFileForSupplierLocally(RequestForConfirmation request)
        {
            try
            {
                using (FileStream fs = File.OpenWrite(
                    $"{CoreSettings.b2bDataLocalDir}{CoreSettings.SuppliersPath}/{request.FTPSupplierFolder}{CoreSettings.SupplierOrdersPath}/order{request.Code}.xml"))
                {
                    XMLProcessor.SaveRequestXMLToStream(request, fs);
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
