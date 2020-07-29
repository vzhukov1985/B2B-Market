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
        private readonly static string HTTPServerUrl = "http://localhost";

        static readonly HttpClient httpClient = new HttpClient();

        public static async Task<byte[]> GetProductPictureAsync(Guid guid)
        {
            try
            {
                return await httpClient.GetByteArrayAsync(HTTPServerUrl + "/Pictures/Products/Matched/" + guid.ToString() + ".png");
            }
            catch (HttpRequestException)
            {
                return Encoding.ASCII.GetBytes("NoPicture");
            }
        }

        public static async Task<byte[]> GetTopCategoryPictureAsync(Guid guid)
        {
            try
            {
                return await httpClient.GetByteArrayAsync(HTTPServerUrl + "/Pictures/TopCategories/" + guid.ToString() + ".png");
            }
            catch (HttpRequestException)
            {
                return Encoding.ASCII.GetBytes("NoPicture");
            }
        }

        public static async Task<byte[]> GetSupplierPictureAsync(Guid guid)
        {
            try
            {
                return await httpClient.GetByteArrayAsync(HTTPServerUrl + "/Pictures/Suppliers/" + guid.ToString() + ".png");
            }
            catch (HttpRequestException)
            {
                return Encoding.ASCII.GetBytes("NoPicture");
            }
        }

    }
}
