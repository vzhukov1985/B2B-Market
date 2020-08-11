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
            return new Uri(CoreSettings.HTTPServerUrl + CoreSettings.MatchedProductsPicturesPath + "/"+ guid.ToString() + CoreSettings.PictureExtension);
        }

        public static Uri GetTopCategoryPictureUri(Guid guid)
        {
            return new Uri(CoreSettings.HTTPServerUrl + CoreSettings.TopCategoriesPicturePath + "/" + guid.ToString() + CoreSettings.PictureExtension);
        }

        public static Uri GetSupplierPictureUri(Guid guid)
        {
            return new Uri(CoreSettings.HTTPServerUrl + CoreSettings.SuppliersPicturePath + "/" + guid.ToString() + CoreSettings.PictureExtension);
        }
    }
}
