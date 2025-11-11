using Microsoft.Extensions.Configuration;
using SNR_BGC.Interface;
using SNR_BGC.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SNR_BGC.Services
{
    public class PasswordGeneratorService : IPasswordGeneratorService
    {
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "0123456789";
        private const string Symbols = "!@#$%&-_=.";

        public string GeneratePassword(int length = 12)
        {
            if (length < 8)
                length = 8; // enforce minimum length

            var rand = RandomNumberGenerator.Create();

            // Ensure the password contains at least one of each requirement
            var requiredChars = new StringBuilder();
            requiredChars.Append(GetRandomChar(Uppercase));
            requiredChars.Append(GetRandomChar(Lowercase));
            requiredChars.Append(GetRandomChar(Numbers));
            requiredChars.Append(GetRandomChar(Symbols));

            // Fill the rest randomly
            string allChars = Uppercase + Lowercase + Numbers + Symbols;
            while (requiredChars.Length < length)
            {
                requiredChars.Append(GetRandomChar(allChars));
            }

            // Shuffle result to avoid predictable positions
            return Shuffle(requiredChars.ToString(), rand);
        }

        private char GetRandomChar(string set)
        {
            byte[] buffer = new byte[4];
            RandomNumberGenerator.Fill(buffer);
            uint num = BitConverter.ToUInt32(buffer, 0);
            return set[(int)(num % (uint)set.Length)];
        }

        private string Shuffle(string input, RandomNumberGenerator rng)
        {
            var array = input.ToCharArray();
            byte[] buffer = new byte[4];

            for (int i = array.Length - 1; i > 0; i--)
            {
                RandomNumberGenerator.Fill(buffer);
                int j = BitConverter.ToInt32(buffer, 0) & int.MaxValue;
                j %= (i + 1);

                (array[i], array[j]) = (array[j], array[i]);
            }
            return new string(array);
        }
    }
}
