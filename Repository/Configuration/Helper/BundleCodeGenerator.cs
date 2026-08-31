using System.Security.Cryptography;
using System.Text;

namespace ERP.Repository.Configuration.Helper
{
    public class BundleCodeGenerator
    {
        private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static string GenerateBundleCode()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            var builder = new StringBuilder();

            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[8];
            rng.GetBytes(buffer);

            for (int i = 0; i < buffer.Length; i++)
            {
                builder.Append(Base62Chars[buffer[i] % Base62Chars.Length]);
            }

            return builder.ToString();
        }
    }
}
