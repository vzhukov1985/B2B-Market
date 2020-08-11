using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class HTTPManager
    {
        public static Uri GetMatchedProductPictureUri(Guid guid)
        {
            return new Uri(B2BPaths.HTTPServerUrl + B2BPaths.MatchedProductsPicturesPath + "/"+ guid.ToString() + B2BPaths.PictureExtension);
        }

        public static Uri GetTopCategoryPictureUri(Guid guid)
        {
            return new Uri(B2BPaths.HTTPServerUrl + B2BPaths.TopCategoriesPicturePath + "/" + guid.ToString() + B2BPaths.PictureExtension);
        }

        public static Uri GetSupplierPictureUri(Guid guid)
        {
            return new Uri(B2BPaths.HTTPServerUrl + B2BPaths.SuppliersPicturePath + "/" + guid.ToString() + B2BPaths.PictureExtension);
        }
    }
}
