using System.Security.Cryptography;
using System.Text;

namespace its_bot.Services
{
    public class EncodingToSha256
    {
        public static string ComputeSha256Hash(string stringData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(stringData));
                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
