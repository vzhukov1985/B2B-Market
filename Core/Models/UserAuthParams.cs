using System;
namespace Core.Models
{
    public class UserAuthParams
    {
        public AuthType AuthType { get; set; }
        public string Login { get; set; }
        public string PasswordOrPin { get; set; }
    }

    public enum AuthType
    {
        ByPassword,
        ByPIN,
        ByBiometric
    }
}
