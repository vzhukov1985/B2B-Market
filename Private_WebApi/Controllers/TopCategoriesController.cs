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
    [Route("api/topcategories")]
    public class TopCategoriesController:Controller
    {
        private readonly MarketDbContext db;

        public TopCategoriesController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTopCategories()
        {
            try
            {
                return Ok(await db.TopCategories.ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
