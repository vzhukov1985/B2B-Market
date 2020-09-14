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

              string StringToEncrypt = "server=192.168.1.100;UserId=Admin;Password=MySQLPassword;database=b2bmarket;";
              string Password = CoreSettings.DbConnectionSalt;
             Console.WriteLine(StringCipher.Encrypt(StringToEncrypt, Password));

            //string Password = "ap~G3te!d_2G";
            //Console.WriteLine(Authentication.HashPassword(Password));
        }
    }
}
