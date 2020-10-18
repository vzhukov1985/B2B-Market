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
    [Route("api/productcategories")]
    public class ProductCategoriesController : Controller
    {
        private MarketDbContext db;

        public ProductCategoriesController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpPost("getbasedonmidcats")]
        public async Task<IActionResult> GetProductCategoriesBasedOnMidCategories([FromQuery] bool onlyguids, [FromBody] List<Guid> midCategoriesIds)
        {
            try
            {
                IQueryable<ProductCategory> query = db.ProductCategories;

                if (midCategoriesIds != null)
                    query = query.Where(pc => midCategoriesIds.Contains(pc.MidCategoryId));

                if (onlyguids)
                    return Ok(await query.Select(pc => pc.Id).ToListAsync());

                return Ok(await query.ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
