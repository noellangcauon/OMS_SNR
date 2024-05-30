using System;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.External.ShopeeWebApi
{

    internal static class ShopeeApiUtil
    {
        public static string SignAuthRequest(string partnerKey, string partnerId, string apiPath, string timestamp)
        {
            using var hashAlgorithm = new HMACSHA256(key: Encoding.UTF8.GetBytes(partnerKey));

            byte[] computedHash = hashAlgorithm.ComputeHash(buffer: Encoding.UTF8.GetBytes($"{partnerId}{apiPath}{timestamp}"));

            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }

        public static string SignShopRequest(string partnerId, string apiPath, string timestamp, string access_token, long shopid, string partnerKey)
        {
            using var hashAlgorithm = new HMACSHA256(key: Encoding.UTF8.GetBytes(partnerKey));

            byte[] computedHash = hashAlgorithm.ComputeHash(buffer: Encoding.UTF8.GetBytes($"{partnerId}{apiPath}{timestamp}{access_token}{shopid}"));

            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }

        public static long ToTimestamp(this DateTime dateTime) => ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }
}