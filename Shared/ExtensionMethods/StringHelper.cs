using System.Security.Cryptography;
using System.Text;

namespace System
{
    public static class StringHelper
    {
        public static string? HashPassword256(this string input)
        {
            var passwordHash = input;
            string hash;

            byte[] bytes = Encoding.UTF8.GetBytes(passwordHash);
            byte[] hashBytes = SHA256.HashData(bytes);

            hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hash;
        }
    }
}
