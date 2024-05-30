using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.External.ShopeeWebApi
{
    internal class ShopeeApiUtils
    {
        private static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }

        private static string SHA256HexHashString(string StringIn)
        {
            string hashString;
            using (var sha256 = SHA256Managed.Create())
            {
                var hash = sha256.ComputeHash(Encoding.Default.GetBytes(StringIn));
                hashString = ToHex(hash, false);
            }
            //dsdasdasdasdasdasdas
            return hashString;
        }

        public static byte[] HashHMACHex(string key, string message)
        {
            byte[] keyByte = new ASCIIEncoding().GetBytes(key);
            byte[] messageBytes = new ASCIIEncoding().GetBytes(message);

            return new HMACSHA256(keyByte).ComputeHash(messageBytes); ;
        }

        public static string Sign(string url, string body, string key)
        {
            string signatureBaseString = $"{url}|{body}";

            HMACSHA256 hmac = new HMACSHA256(Encoding.Default.GetBytes(key));

            var hash = hmac.ComputeHash(Encoding.Default.GetBytes(signatureBaseString));

            return ToHex(hash, false);
        }
    }
}
