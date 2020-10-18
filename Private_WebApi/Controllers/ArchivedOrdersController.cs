using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Private_WebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ClientUser")]
    [Route("/api/archivedorders")]
    public class ArchivedOrdersController: Controller
    {
        private MarketDbContext db;

        public ArchivedOrdersController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            try
            {
                return Ok(await db.ArchivedRequestStatusTypes.ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
