using Core.Services;
using System;

namespace StringEncrypter
{
    class Program
    {
        static void Main()
        {
            string StringToEncrypt = "server=localhost;UserId=root;Password=MySQLPassword;database=b2bmarket;";
            string Password = "gy!34q2.gteas_";
            Console.WriteLine(StringCipher.Encrypt(StringToEncrypt, Password));
        }
    }
}
