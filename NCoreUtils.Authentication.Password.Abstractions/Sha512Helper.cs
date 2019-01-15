using System.Security.Cryptography;
using System.Text;

namespace NCoreUtils.Authentication
{
    static class Sha512Helper
    {
        static readonly string[] _hex = new string[256];

		static Sha512Helper()
		{
			for (var low = 0; low < 16; ++low)
			{
				for (var high = 0; high < 16; ++high)
				{
					byte x = (byte)(low + (high * 16));
					_hex[x] = x.ToString("X2");
				}
			}
		}

		public static string CalculateHash(byte[] data)
		{
			using (var sha = SHA512CryptoServiceProvider.Create())
			{
				var hash = sha.ComputeHash(data);
				var builder = new StringBuilder(hash.Length * 2);
				for (var i = 0; i < hash.Length; ++i)
				{
					builder.Append(_hex[hash[i]]);
				}
				return builder.ToString();
			}
		}

        public static string CalculateHash(string data)
            => CalculateHash(Encoding.ASCII.GetBytes(data));
    }
}