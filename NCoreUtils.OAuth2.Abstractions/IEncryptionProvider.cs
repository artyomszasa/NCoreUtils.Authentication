using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.OAuth2
{
    public interface IEncryptionProvider
    {
        Task<byte[]> Encrypt(byte[] plainData, CancellationToken cancellationToken = default(CancellationToken));

        Task<byte[]> Decrypt(byte[] cipherData, CancellationToken cancellationToken = default(CancellationToken));
    }
}