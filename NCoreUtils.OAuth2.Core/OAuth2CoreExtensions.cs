using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2
{
    public static class OAuth2CoreExtensions
    {
        public static async Task<OAuth2Response> RefreshTokenAsync(
            this OAuth2Core core,
            string refreshToken,
            IEncryptionProvider encryptionProvider,
            CancellationToken cancellationToken)
        {
            Token token;
            try
            {
                core._logger.LogTrace("Decrypting refresh token: \"{0}\".", refreshToken);
                token = await encryptionProvider.DecryptTokenFromBase64(refreshToken, cancellationToken).ConfigureAwait(false);
                core._logger.LogTrace("Successfully decrypted refresh token: \"{0}\".", refreshToken);
            }
            catch (Exception exn)
            {
                core._logger.LogTrace(exn, "Failed to decrypt refresh token: \"{0}\".", refreshToken);
                throw new OAuth2Exception(OAuth2Error.InvalidGrant, OAuth2ErrorMessages.InvalidRefreshToken, exn);
            }
            var accessToken = await core.RefreshTokenAsync(token, cancellationToken).ConfigureAwait(false);
            core._logger.LogTrace("Issued new access token for user #\"{0}\" from refresh token.", accessToken.Id);
            var encryptedAccessToken = await encryptionProvider.EncryptTokenToBase64(accessToken, cancellationToken).ConfigureAwait(false);
            return new OAuth2Response(
                accessToken: encryptedAccessToken,
                tokenType: "Bearer",
                expiresIn: (int)Math.Round((accessToken.ExpiresAt - accessToken.IssuedAt).TotalSeconds),
                refreshToken: null);
        }

        public static async Task<OAuth2Response> AuthenticateByPasswordAsync(
            this OAuth2Core core,
            int clientApplicationId,
            string username,
            string password,
            string scopes,
            IEncryptionProvider encryptionProvider,
            CancellationToken cancellationToken)
        {
            var tokens = await core.AuthenticateByPasswordAsync(clientApplicationId, username, password, scopes, cancellationToken).ConfigureAwait(false);
            return await OAuth2Response.FromTokensAsync(tokens.accessToken, tokens.refreshToken, encryptionProvider, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<OAuth2Response> AuthenticateByCodeAsync(
            this OAuth2Core core,
            int clientApplicationId,
            string redirectUri,
            string code,
            IEncryptionProvider encryptionProvider,
            CancellationToken cancellationToken)
        {
            var decryptedCode = await encryptionProvider.Decrypt(Convert.FromBase64String(code)).ConfigureAwait(false);
            var codeGuid = new Guid(decryptedCode);
            var tokens = await core.AuthenticateByCodeAsync(clientApplicationId, redirectUri, codeGuid, cancellationToken).ConfigureAwait(false);
            return await OAuth2Response.FromTokensAsync(tokens.accessToken, tokens.refreshToken, encryptionProvider, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> CreateAuthorizationCodeByPasswordAsync(
            this OAuth2Core core,
            int clientApplicationId,
            string username,
            string password,
            string scopes,
            IEncryptionProvider encryptionProvider,
            CancellationToken cancellationToken)
        {
            var codeGuid = await core.CreateAuthorizationCodeByPasswordAsync(clientApplicationId, username, password, scopes, cancellationToken).ConfigureAwait(false);
            var encryptedCode = await encryptionProvider.Encrypt(codeGuid.ToByteArray()).ConfigureAwait(false);
            return Convert.ToBase64String(encryptedCode);
        }
    }
}