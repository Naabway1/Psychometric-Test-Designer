using System.Security.Cryptography;
using System.Text;

namespace Psychometric_Test_Designer.Core
{
    public class TokenGenerator
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public string Generate(int length = 50)
        {
            var result = new StringBuilder(length);
            var buffer = new byte[sizeof(uint)];

            using var rng = RandomNumberGenerator.Create();

            while (result.Length < length)
            {
                rng.GetBytes(buffer);
                var num = BitConverter.ToUInt32(buffer, 0);
                var idx = num % Chars.Length;
                result.Append(Chars[(int)idx]);
            }

            return result.ToString();
        }
    }
}

