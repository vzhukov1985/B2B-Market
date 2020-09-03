using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCrypt.Net;
using Core.DBModels;

namespace Core.Services
{
    public static class Authentication
    {
        private static readonly string hardcodedPwdSalt = CoreSettings.ClientUserPwdSalt;
        private static readonly string hardcodedPINSalt = CoreSettings.ClientUserPINSalt;
        private static readonly Random random = new Random();
        public static bool IsLoginAlreadyExists(string loginToCheck, Guid userId)
        {
            using (MarketDbContext db = new MarketDbContext())
            {
                List<ClientUser> clientUsers = db.ClientsUsers.ToList();
                foreach (ClientUser userCheck in clientUsers)
                {
                    if ((userCheck.Login == loginToCheck) && (userCheck.Id != userId))
                        return true;
                }
            }
            return false;
        }

        public static string GenerateUniqueLogin()
        {
            string login;
            int freeNum = 1;
            login = "login" + freeNum.ToString();
            using (MarketDbContext db = new MarketDbContext())
            {
                List<ClientUser> clientUsers = db.ClientsUsers.ToList();
                while (((clientUsers.Where(l => (l.Login == login)).ToList()).Count) > 0)
                {
                    freeNum++;
                    login = "login" + freeNum.ToString();
                } 
            }

            return login;
        }

        public static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password+hardcodedPwdSalt, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public static string HashPIN(string PIN)
        {
            return BCrypt.Net.BCrypt.HashPassword(PIN + hardcodedPINSalt, BCrypt.Net.BCrypt.GenerateSalt());
        }


        public static bool CheckPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password + hardcodedPwdSalt, passwordHash);
        }
        public static bool CheckPIN(string PIN, string PINHash)
        {
            return BCrypt.Net.BCrypt.Verify(PIN + hardcodedPINSalt, PINHash);
        }
    }
}
