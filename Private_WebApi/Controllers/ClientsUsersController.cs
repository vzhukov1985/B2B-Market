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
    [Route("api/clientsusers")]
    public class ClientsUsersController: Controller
    {
        private MarketDbContext db;

        public ClientsUsersController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool onlylogins)
        {
            try
            {
                IQueryable<ClientUser> query = db.ClientsUsers;

                if (onlylogins)
                    return Ok(await query.Select(cu => cu.Login).ToListAsync());

                return Ok(await query.ToListAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
