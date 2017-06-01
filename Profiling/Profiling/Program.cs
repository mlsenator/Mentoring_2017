using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            var password = "KokoKo_123!";

            byte[] salt = new byte[16];

            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt);
            }

            var hash = GeneratePasswordHashUsingSalt(password, salt);
            Console.WriteLine(hash);

            Console.ReadKey();
        }

        public static string GeneratePasswordHashUsingSalt(string passwordText, byte[] salt)
        {

            var iterate = 10000;
            var pbkdf2 = new Rfc2898DeriveBytesCustom(passwordText, salt, iterate);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            var passwordHash = Convert.ToBase64String(hashBytes);

            return passwordHash;
        }
    }
}
