using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2
{
    public static class EncryptionProviderExtensions
    {
        static readonly UTF8Encoding _utf8 = new UTF8Encoding(false);

        public static Task<byte[]> EncryptToken(this IEncryptionProvider encryption, Token token, CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] plainData;
            using (var buffer = new MemoryStream())
            {
                using (var gzip = new GZipStream(buffer, CompressionLevel.Optimal, true))
                using (var writer = new BinaryWriter(gzip, _utf8, true))
                {
                    token.WriteTo(writer);
                    writer.Flush();
                }
                plainData = buffer.ToArray();
            }
            return encryption.Encrypt(plainData, cancellationToken);
        }

        public static async Task<Token> DecryptToken(this IEncryptionProvider encryption, byte[] cipherData, CancellationToken cancellationToken = default(CancellationToken))
        {
            var plainData = await encryption.Decrypt(cipherData, cancellationToken).ConfigureAwait(false);
            using (var buffer = new MemoryStream(plainData, false))
            using (var gzip = new GZipStream(buffer, CompressionMode.Decompress, true))
            using (var reader = new BinaryReader(gzip, _utf8, true))
            {
                return new Token(reader);
            }
        }

        public static async Task<string> EncryptTokenToBase64(this IEncryptionProvider encryption, Token token, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cipherData = await encryption.EncryptToken(token, cancellationToken).ConfigureAwait(false);
            return Convert.ToBase64String(cipherData);
        }

        public static Task<Token> DecryptTokenFromBase64(this IEncryptionProvider encryption, string base64ChipherData, CancellationToken cancellationToken = default(CancellationToken))
        {
            return encryption.DecryptToken(Convert.FromBase64String(base64ChipherData), cancellationToken);
        }
    }
}