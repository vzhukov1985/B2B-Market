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

              string StringToEncrypt = "server=192.168.1.3;UserId=Admin;Password=MySQLPassword;database=b2bmarket;";
              string Password = "gy!34q2.gteas_";
             Console.WriteLine(StringCipher.Encrypt(StringToEncrypt, Password));

            //string Password = "ap~G3te!d_2G";
            //Console.WriteLine(Authentication.HashPassword(Password));
        }
    }
}
