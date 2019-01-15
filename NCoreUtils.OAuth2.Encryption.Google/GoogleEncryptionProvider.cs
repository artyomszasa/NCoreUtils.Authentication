using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.CloudKMS.v1;
using Google.Apis.CloudKMS.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace NCoreUtils.OAuth2
{
    public class GoogleEncryptionProvider : IEncryptionProvider
    {
        static readonly GoogleCredential _googleCredential;

        static GoogleCredential InitializeGoogleCredential()
        {
            GoogleCredential credential = GoogleCredential.GetApplicationDefault();
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped(new[]
                {
                    Google.Apis.CloudKMS.v1.CloudKMSService.Scope.CloudPlatform
                });
            }
            return credential;
        }

        static GoogleEncryptionProvider()
        {
            _googleCredential = InitializeGoogleCredential();
        }

        static CloudKMSService CreateKmsService()
        {
            return new CloudKMSService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _googleCredential,
                GZipEnabled = true
            });
        }

        readonly string _cryptoKey;

        public GoogleEncryptionProvider(IOptions<GoogleEncryptionConfiguration> configurationOptions)
        {
            if (configurationOptions == null)
            {
                throw new System.ArgumentNullException(nameof(configurationOptions));
            }
            var conf = configurationOptions.Value;
            _cryptoKey = $"projects/{conf.ProjectId}/locations/{conf.LocationId}/keyRings/{conf.KeyRingId}/cryptoKeys/{conf.CryptoKeyId}";
        }

        public async Task<byte[]> Decrypt(byte[] cipherData, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var cloudKms = CreateKmsService())
            {
                var decryptRequest = new DecryptRequest();
                decryptRequest.Ciphertext = Convert.ToBase64String(cipherData);
                var response = await cloudKms.Projects.Locations.KeyRings.CryptoKeys.Decrypt(name: _cryptoKey, body: decryptRequest).ExecuteAsync(cancellationToken);
                return Convert.FromBase64String(response.Plaintext);
            }
        }

        public async Task<byte[]> Encrypt(byte[] plainData, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var cloudKms = CreateKmsService())
            {
                var encryptRequest = new EncryptRequest();
                encryptRequest.Plaintext = Convert.ToBase64String(plainData);
                var response = await cloudKms.Projects.Locations.KeyRings.CryptoKeys.Encrypt(name: _cryptoKey, body: encryptRequest).ExecuteAsync(cancellationToken);
                return Convert.FromBase64String(response.Ciphertext);
            }
        }
    }
}