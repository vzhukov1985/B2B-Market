using Core.DBModels;
using Core.Services;
using System;

namespace StringEncrypter
{
    class Program
    {
        static void Main()
        {
            using (MarketDbContext db = new MarketDbContext())
            {

            }
            // D12t7G

            var a = Authentication.HashPassword("D12t7G");
            //  string StringToEncrypt = "server=localhost;UserId=Admin;Password=MySQLPassword;database=b2bmarket;";
            //  string Password = CoreSettings.DbConnectionSalt;
            // Console.WriteLine(StringCipher.Encrypt(StringToEncrypt, Password));

            //string Password = "ap~G3te!d_2G";
            //Console.WriteLine(Authentication.HashPassword(Password));
        }
    }
}
