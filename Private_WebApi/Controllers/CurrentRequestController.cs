using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Private_WebApi.Controllers
{
    [ApiController]
    [Route("currentrequest")]
    public class CurrentRequestController
    {
        [HttpPost("proceed")]
        public async Task<IActionResult> ProceedRequest()
    }
}
