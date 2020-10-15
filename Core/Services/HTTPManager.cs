using Core.DBModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class HTTPManager
    {
        static HttpClient httpClient = new HttpClient();

        //Private Api

        public static Uri GetMatchedProductPictureUri(Guid guid)
        {
            return new Uri($"{CoreSettings.PrivateAPIUrl}/images/product/matched/{guid}");
        }

        public static Uri GetTopCategoryPictureUri(Guid guid)
        {
            return new Uri($"{CoreSettings.PrivateAPIUrl}/images/topcategory/{guid}");
        }

        public static Uri GetSupplierPictureUri(Guid guid)
        {
            return new Uri($"{CoreSettings.PrivateAPIUrl}/images/supplier/{guid}");
        }
    }
}
