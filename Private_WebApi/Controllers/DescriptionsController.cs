using System;
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
    [Route("api/proddesc")]
    public class DescriptionsController : Controller
    {
        MarketDbContext db;

        public DescriptionsController(MarketDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpPost("getbyproduct")]
        public async Task<IActionResult> GetDescriptionByProduct([FromBody] Guid productId)
        {
            try
            {
                return Ok(await db.ProductDescriptions.Where(pd => pd.ProductId == productId).Select(pd => pd.Text).FirstOrDefaultAsync());
            }
            catch
            {
                return StatusCode(500);
            }
        }

    }
}
