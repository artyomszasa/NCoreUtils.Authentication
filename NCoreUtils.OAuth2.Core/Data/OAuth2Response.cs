using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NCoreUtils.OAuth2.Data
{
    public sealed class OAuth2Response
    {
        static Task<string> EncryptOrNull(Token token, IEncryptionProvider encryptionProvider, CancellationToken cancellationToken)
        {
            if (null == token)
            {
                return Task.FromResult<string>(null);
            }
            return encryptionProvider.EncryptTokenToBase64(token, cancellationToken);
        }

        public static async Task<OAuth2Response> FromTokensAsync(
            Token accessToken,
            Token refreshToken,
            IEncryptionProvider encryptionProvider,
            CancellationToken cancellationToken)
        {
            var encryptedAccessToken = await encryptionProvider.EncryptTokenToBase64(accessToken, cancellationToken).ConfigureAwait(false);
            var encryptedRefreshToken = await EncryptOrNull(refreshToken, encryptionProvider, cancellationToken).ConfigureAwait(false);
            return new OAuth2Response(
                accessToken: encryptedAccessToken,
                tokenType: "Bearer",
                expiresIn: (int)Math.Round((accessToken.ExpiresAt - accessToken.IssuedAt).TotalSeconds),
                refreshToken: encryptedRefreshToken);
        }

        [JsonProperty("access_token")]
        public string AccessToken { get; }

        [JsonProperty("token_type")]
        public string TokenType { get; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; }

        public OAuth2Response(string accessToken, string tokenType, int expiresIn, string refreshToken)
        {
            AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            TokenType = tokenType ?? throw new ArgumentNullException(nameof(tokenType));
            ExpiresIn = expiresIn;
            RefreshToken = refreshToken;
        }
    }
}