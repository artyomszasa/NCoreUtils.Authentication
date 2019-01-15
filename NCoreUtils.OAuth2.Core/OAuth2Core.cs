using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCoreUtils.Authentication;
using NCoreUtils.Data;
using NCoreUtils.Linq;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2
{
    public class OAuth2Core
    {
        static readonly Regex _separator = new Regex(@"\s*,\s*", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        readonly LoginAuthenticator _loginAuthenticator;

        readonly IDataRepository<RefreshToken> _refreshTokenRepository;

        readonly IDataRepository<AuthorizationCode, Guid> _authorizationCodeRepository;

        readonly OAuth2Configuration _configuration;

        internal readonly ILogger _logger;

        public OAuth2Core(
            LoginAuthenticator loginAuthenticator,
            IOptions<OAuth2Configuration> configurationOptions,
            ILogger<OAuth2Core> logger,
            IDataRepository<RefreshToken> refreshTokenRepository,
            IDataRepository<AuthorizationCode, Guid> authorizationCodeRepository)
        {
            _loginAuthenticator = loginAuthenticator ?? throw new ArgumentNullException(nameof(loginAuthenticator));
            if (configurationOptions == null)
            {
                throw new ArgumentNullException(nameof(configurationOptions));
            }
            _configuration = configurationOptions.Value ?? throw new ArgumentNullException($"{nameof(configurationOptions)}.{nameof(configurationOptions.Value)}");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _authorizationCodeRepository = authorizationCodeRepository ?? throw new ArgumentNullException(nameof(authorizationCodeRepository));
        }

        async Task<(Token accessToken, Token refreshToken)> CreateTokensAsync(int userId, string[] grantedScopes)
        {
            var issuedAt = DateTimeOffset.Now;
            // create access token
            var accessToken = new Token(
                id: userId.ToString(),
                issuedAt: issuedAt,
                expiresAt: issuedAt + _configuration.AccessTokenExpiry,
                scopes: grantedScopes);
            // create refreshToken
            var refreshToken = new Token(
                id: userId.ToString(),
                issuedAt: issuedAt,
                expiresAt: issuedAt + _configuration.RefreshTokenExpiry,
                scopes: grantedScopes);
            // persist refresh token
            Array.Sort(grantedScopes, StringComparer.OrdinalIgnoreCase); // scopes stored as sorted array
            await _refreshTokenRepository.PersistAsync(new RefreshToken
            {
                UserId = userId,
                State = State.Public,
                IssuedAt = refreshToken.IssuedAt.UtcTicks,
                ExpiresAt = refreshToken.ExpiresAt.UtcTicks,
                Scopes = string.Join(",", grantedScopes)
            }).ConfigureAwait(false);
            return (accessToken, refreshToken);
        }

        async Task<ClaimCollection> AuthenticateUser(string username, string password, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogTrace("Authenticating \"{0}\" using password authentication.", username);
            var claims = await _loginAuthenticator.AuthenticateAsync("password", $"{username}:{password}", cancellationToken).ConfigureAwait(false);
            if (null == claims)
            {
                _logger.LogDebug("Failed to authenticate \"{0}\" using password authentication.", username);
                throw new OAuth2Exception(OAuth2Error.AccessDenied, OAuth2ErrorMessages.InvalidUserCredentials);
            }
            _logger.LogDebug("Successfully authenticated \"{0}\" using password authentication.", username);
            return claims;
        }

        (int userId, string[] grantedScopes) ValidateUserAndScopes(ClaimCollection claims, int clientApplicationId, string scopes)
        {
            var idString = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Sid)?.Value;
            if (string.IsNullOrEmpty(idString) || !int.TryParse(idString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                throw new OAuth2Exception(OAuth2Error.ServerError, OAuth2ErrorMessages.InvalidUser);
            }
            // validate that authenticated user belongs to the actual client application
            var caidString = claims.FirstOrDefault(claim => claim.Type == OAuth2ClientApplicationClaims.ClientApplicationId)?.Value;
            if (string.IsNullOrEmpty(caidString) || !int.TryParse(caidString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var caid) || caid != clientApplicationId)
            {
                throw new OAuth2Exception(OAuth2Error.AccessDenied, OAuth2ErrorMessages.InvalidUserCredentials);
            }

            // validate that user has sufficient permissions to grant requested scopes
            var availableScopes = new HashSet<string>(claims
                .Where(claim => claim.Type == claims.RoleClaimType)
                .Select(claim => claim.Value),
                StringComparer.OrdinalIgnoreCase);
            string[] grantedScopes;
            if (string.IsNullOrWhiteSpace(scopes))
            {
                _logger.LogTrace("No requested scopes, granting all available scopes for #{0}: {1}.", id, string.Join(",", availableScopes));
                grantedScopes = availableScopes.ToArray();
            }
            else
            {
                _logger.LogTrace("Validating requested scopes for #{0}: {1}.", id, scopes);
                var requestedScopes = _separator.Split(scopes);
                foreach (var requestedScope in requestedScopes)
                {
                    if (!availableScopes.Contains(requestedScope))
                    {
                        throw new OAuth2Exception(OAuth2Error.InvalidScope, OAuth2ErrorMessages.UnsufficientPermissionsToGrant(requestedScope));
                    }
                }
                _logger.LogTrace("Successfully validated requested scopes for #{0}: {1}.", id, scopes);
                grantedScopes = Array.ConvertAll(requestedScopes, scope => scope.ToLower(CultureInfo.InvariantCulture));
            }
            return (id, grantedScopes);
        }

        public async Task<Guid> CreateAuthorizationCodeByPasswordAsync(int clientApplicationId, string username, string password, string scopes, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var claims = await AuthenticateUser(username, password, cancellationToken).ConfigureAwait(false);
            var (userId, grantedScopes) = ValidateUserAndScopes(claims, clientApplicationId, scopes);
            var now = DateTimeOffset.Now;
            Array.Sort(grantedScopes, StringComparer.OrdinalIgnoreCase); // scopes stored as sorted array
            var authCode = await _authorizationCodeRepository.PersistAsync(new AuthorizationCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                IssuedAt = now.UtcTicks,
                ExpiresAt = (now + _configuration.AuthorizationCodeExpiry).UtcTicks,
                Scopes = string.Join(",", grantedScopes)
            }).ConfigureAwait(false);
            return authCode.Id;
        }

        public async Task<(Token accessToken, Token refreshToken)> AuthenticateByPasswordAsync(int clientApplicationId, string username, string password, string scopes, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var claims = await AuthenticateUser(username, password, cancellationToken).ConfigureAwait(false);
            var (userId, grantedScopes) = ValidateUserAndScopes(claims, clientApplicationId, scopes);
            return await CreateTokensAsync(userId, grantedScopes).ConfigureAwait(false);
        }

        public Task<(Token accessToken, Token refreshToken)> AuthenticateByCodeAsync(
            int clientApplicationId,
            string redirectUri,
            Guid authorizationCodeGuid,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _authorizationCodeRepository.Context.TransactedAsync(
                isolationLevel: IsolationLevel.ReadCommitted,
                cancellationToken: cancellationToken,
                action: async () =>
                {
                    var authCode = await _authorizationCodeRepository.Items
                        .Where(code => code.Id == authorizationCodeGuid
                                        && code.RedirectUri == redirectUri
                                        && code.User.ClientApplictionId == clientApplicationId)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
                    if (null == authCode)
                    {
                        throw new OAuth2Exception(OAuth2Error.InvalidGrant, OAuth2ErrorMessages.InvalidAuthorizationCode);
                    }
                    var tokens = await CreateTokensAsync(authCode.UserId, authCode.Scopes.Split(',')).ConfigureAwait(false);
                    await _authorizationCodeRepository.RemoveAsync(authCode);
                    return tokens;
                });
        }

        public Task<Token> RefreshTokenAsync(Token refreshToken, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _refreshTokenRepository.Context.TransactedAsync(
                isolationLevel: IsolationLevel.ReadCommitted,
                cancellationToken: cancellationToken,
                action: async () =>
                {
                    var grantedScopes = refreshToken.Scopes.ToArray();
                    Array.Sort(grantedScopes, StringComparer.OrdinalIgnoreCase); // scopes stored as sorted array
                    var scopes = string.Join(",", grantedScopes);
                    var userId = string.IsNullOrWhiteSpace(refreshToken.Id) ? -1 : (int.TryParse(refreshToken.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uid) ? uid : -1);
                    var issuedAtUtcTicks = refreshToken.IssuedAt.UtcTicks;
                    var expiresAtUtcTicks = refreshToken.ExpiresAt.UtcTicks;
                    var rtoken = await _refreshTokenRepository.Items
                        .Where(rt => rt.State == State.Public && rt.UserId == userId && rt.IssuedAt == issuedAtUtcTicks
                                        && rt.ExpiresAt == expiresAtUtcTicks && rt.Scopes == scopes)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
                    if (null == rtoken)
                    {
                        throw new OAuth2Exception(OAuth2Error.InvalidGrant, OAuth2ErrorMessages.InvalidRefreshToken);
                    }
                    var now = DateTimeOffset.Now;
                    // create access token
                    var accessToken = new Token(
                        id: refreshToken.Id,
                        issuedAt: now,
                        expiresAt: now + _configuration.AccessTokenExpiry,
                        scopes: grantedScopes);
                    // update refreshToken.LastUsed
                    rtoken.LastUsed = now.UtcTicks;
                    await _refreshTokenRepository.PersistAsync(rtoken).ConfigureAwait(false);
                    // return access token
                    return accessToken;
                });
        }
    }
}