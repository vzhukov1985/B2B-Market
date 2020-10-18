using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Private_WebApi.Models
{
    public class AuthOptions
    {

        public const string ISSUER = "B2BPrivateWebApi"; // издатель токена
        public const string AUDIENCE = "B2BUser"; // потребитель токена
        const string KEY = "su8!+sd6Re09!ghv8qm.cskowej67!";   // ключ для шифрации
        public const int LIFETIME = 60; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}

