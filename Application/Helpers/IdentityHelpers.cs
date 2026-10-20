using System.Security.Cryptography;
using System.Text;

namespace Application.Helpers
{
    public static class IdentityHelpers
    {
        private const int Pbkdf2Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const string Pbkdf2Prefix = "PBKDF2$";

        public static void GetPasswordHashAndSalt(string password, out string hash, out string salt)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            salt = Convert.ToBase64String(saltBytes);
            var derived = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                KeySize);
            hash = Pbkdf2Prefix + Convert.ToBase64String(derived);
        }

        public static bool ValidatePassword(string password, string hash, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
                return false;

            if (hash.StartsWith(Pbkdf2Prefix, StringComparison.Ordinal))
            {
                var expected = Convert.FromBase64String(hash[Pbkdf2Prefix.Length..]);
                var saltBytes = Convert.FromBase64String(salt);
                var actual = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    saltBytes,
                    Pbkdf2Iterations,
                    HashAlgorithmName.SHA256,
                    KeySize);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }

            // Legacy HMAC-SHA256 fallback for existing users
            using HMAC hMAC = new HMACSHA256(Convert.FromBase64String(salt));
            var legacy = Convert.ToBase64String(hMAC.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(legacy),
                Encoding.UTF8.GetBytes(hash));
        }

        public static string HashResetToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);
        }

        public static string CreateRawResetToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        public static void GetSecretKeyAndAppId(out string secretkey, out string appId)
        {
            var hmac = new HMACSHA256();
            secretkey = Convert.ToBase64String(hmac.Key);
            appId = Guid.NewGuid().ToString();
        }
    }
}
