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
    [Route("api/midcategories")]
    public class MidCategoriesController: Controller
    {
        private readonly MarketDbContext db;

        public MidCategoriesController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetMidCategoriesOfTop([FromQuery] string topcategoryid)
        {
            Guid topCategoryId = Guid.Empty;

            if (!string.IsNullOrEmpty(topcategoryid) && !Guid.TryParse(topcategoryid, out topCategoryId))
                return BadRequest();

            try
            {
                IQueryable<MidCategory> query = db.MidCategories;

                if (!string.IsNullOrEmpty(topcategoryid))
                    query = query.Where(mc => mc.TopCategoryId == topCategoryId);

                return Ok(await query.ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
