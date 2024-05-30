
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SNR_BGC.Utilities
{
    public static class EncryptorDecryptor
    {
        //Time And Attendance
        //MD5 Hash = 539d6263004e6f325ef1371e1c2dc112
        //SHA1 Hash = 68808f080535f9712b1b21cacc06da91f61d3d5c
        public static string EncryptionKey = "68808f080535f9712b1b21cacc06da91f61d3d5c";

        public static string Encrypt(string cryptext)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(cryptext);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    cryptext = Convert.ToBase64String(ms.ToArray());
                }
            }
            return cryptext;
        }

        public static string Decrypt(string ciphertext)
        {
            byte[] cipherBytes = Convert.FromBase64String(ciphertext);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    ciphertext = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return ciphertext;
        }
    }
}
