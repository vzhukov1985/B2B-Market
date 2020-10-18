using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Core.DBModels;
using Core.Models;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Private_WebApi.Models;

namespace Private_WebApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private MarketDbContext db;

        public AuthenticationController(MarketDbContext dbContext)
        {
            db = dbContext;
        }


        [HttpPost]
        public IActionResult Login(UserAuthParams authParams)
        {
            var userAuthInfo = new { Id = "", PasswordHash = "", PinHash = ""};
            try
            {
                userAuthInfo = db.ClientsUsers.Where(cu => cu.Login == authParams.Login).Select(cu => new { Id = cu.Id.ToString(), cu.PasswordHash, cu.PinHash}).FirstOrDefault();
            }
            catch
            {
                return StatusCode(500);
            }

            if (userAuthInfo == null)
                return BadRequest("Invalid login or password");

            switch(authParams.AuthType)
            {
                case AuthType.ByPassword:
                    if (!Authentication.CheckPassword(authParams.PasswordOrPin, userAuthInfo.PasswordHash))
                        return BadRequest("Invalid login or password");
                    break;
                case AuthType.ByPIN:
                    if (string.IsNullOrEmpty(userAuthInfo.PinHash))
                    {
                        return NoContent();
                    }
                    if (!Authentication.CheckPIN(authParams.PasswordOrPin, userAuthInfo.PinHash))
                        return BadRequest("Invalid login or password");
                    break;
                case AuthType.ByBiometric:
                    break;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userAuthInfo.Id),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "ClientUser")
            };

            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: claimsIdentity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(encodedJwt);
        }

    }
}