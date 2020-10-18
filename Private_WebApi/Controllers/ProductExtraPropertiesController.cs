using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DBModels;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Private_WebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ClientUser")]
    [Route("api/peps")]
    public class ProductExtraPropertiesController : Controller
    {
        MarketDbContext db;
        public ProductExtraPropertiesController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpPost("getbyproduct")]
        public async Task<IActionResult> GetProductExtraProperties([FromBody] Guid productId)
        {
            try
            {
                return Ok(await db.ProductExtraProperties
                                  .Where(pep => pep.ProductId == productId)
                                  .Select(pep => new ProductExtraProperty
                                  {
                                      PropertyType = new ProductExtraPropertyType { Name = pep.PropertyType.Name },
                                      Value = pep.Value
                                  })
                                  .ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
