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
    [Route("api/suppliers")]
    public class SuppliersController : Controller
    {
        private MarketDbContext db;

        public SuppliersController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveSuppliersIdNames([FromQuery] string status, [FromQuery] bool onlyidname)
        {
            try
            {
                IQueryable<Supplier> query = db.Suppliers;
                switch (status)
                {
                    case "active":
                        query = query.Where(s => s.IsActive);
                        break;
                    case "inactive":
                        query = query.Where(s => !s.IsActive);
                        break;
                    default:
                        break;
                }

                if (onlyidname)
                    return Ok(await query.Select(s => new SupplierIdName { Id = s.Id, ShortName = s.ShortName }).ToListAsync());

                return Ok(await query.ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
