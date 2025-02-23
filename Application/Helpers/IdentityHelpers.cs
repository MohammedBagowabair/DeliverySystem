using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public class IdentityHelpers
    {
        public static void GetPasswordHashAndSalt(string password, out string hash, out string salt)
        {
            using HMAC hMAC = new HMACSHA256();
            salt = Convert.ToBase64String(hMAC.Key);
            hash = Convert.ToBase64String(hMAC.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
        public static bool ValidatePassword(string password, string hash, string salt)
        {
            using HMAC hMAC = new HMACSHA256(Convert.FromBase64String(salt));
            return Convert.ToBase64String(hMAC.ComputeHash(Encoding.UTF8.GetBytes(password))) == hash;
        }
        public static void GetSecretKeyAndAppId(out string secretkey, out string appId)
        {
            var hmac = new HMACSHA256();
            secretkey = Convert.ToBase64String(hmac.Key);
            appId = Guid.NewGuid().ToString();
        }
    }
}
