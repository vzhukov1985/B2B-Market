using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DBModels;
using Microsoft.AspNetCore.Mvc;

namespace Private_WebApi.Controllers
{
    [Route("changefavorites")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        [HttpPut()]
        public async Task<HttpResponseMessage> Put(KeyValuePair<string,string> userProductGuids)
        {
           Guid userIdGuid, productIdGuid;
            try
            {
                userIdGuid = Guid.Parse(userProductGuids.Key);
                productIdGuid = Guid.Parse(userProductGuids.Value);
            }
            catch
            {
                return null;
            }

            using (MarketDbContext db = new MarketDbContext())
            {
                var fav = db.Favorites.Where(f => f.ClientUserId == userIdGuid && f.ProductId == productIdGuid).FirstOrDefault();

                if (fav == null)
                {
                    db.Favorites.Add(new Favorite { ClientUserId = userIdGuid, ProductId = productIdGuid });
                }
                else
                {
                    db.Favorites.Remove(fav);
                }
                await db.SaveChangesAsync();
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
