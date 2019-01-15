using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NCoreUtils.Authentication
{
    public class LoginAuthentication : ILoginAuthentication
    {
        static readonly char[] _separator = new[] { ',' };

        readonly LoginAuthenticator _loginAuthenticator;

        readonly ILogger _logger;

        public LoginAuthentication(LoginAuthenticator loginAuthenticator, ILogger<LoginAuthentication> logger)
        {
            _loginAuthenticator = loginAuthenticator ?? throw new ArgumentNullException(nameof(loginAuthenticator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ClaimsPrincipal> AuthenticateAsync(string name, string passcode, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            var loginRequests = name.Split(_separator, StringSplitOptions.RemoveEmptyEntries).Map(loginName => new LoginRequest(loginName, passcode));

            var identities = await _loginAuthenticator.AuthenticateAsync(loginRequests)
                .Select(claims => new ClaimsIdentity(
                    claims.Select(claim => new Claim(claim.Type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer)),
                    claims.AuthenticationType,
                    claims.NameClaimType,
                    claims.RoleClaimType))
                .ToList(cancellationToken);

            if (identities.Count > 0)
            {
                _logger.LogDebug("Authenticated successfully ({0} identities).", identities.Count);
                return new ClaimsPrincipal(identities);
            }
            _logger.LogDebug("Authentication failed (no identities has been returned).");
            return null;
        }
    }
}