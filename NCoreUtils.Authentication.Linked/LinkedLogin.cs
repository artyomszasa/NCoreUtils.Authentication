using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Authentication.Linked;
using NCoreUtils.Authentication.Local;

namespace NCoreUtils.Authentication
{
    class LinkedLogin<TUserId> : LocalLogin<TUserId>
    {
        static readonly StringComparer _ord = StringComparer.OrdinalIgnoreCase;

        static readonly ImmutableHashSet<string> _localClaimTypes = ImmutableHashSet.CreateRange(new []
        {
            ClaimTypes.Sid,
            ClaimTypes.Email,
            ClaimTypes.Name,
            ClaimTypes.Surname,
            ClaimTypes.GivenName
        });

        static string GetExternalId(ClaimCollection identity)
        {
            var claim = identity.FirstOrDefault(c => _ord.Equals(c.Type, "id"))
                ?? identity.FirstOrDefault(c => c.Type.EndsWith("/id", StringComparison.OrdinalIgnoreCase))
                ?? identity.FirstOrDefault(c => c.Type.EndsWith("/sub", StringComparison.OrdinalIgnoreCase))
                ?? identity.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
            return claim?.Value;
        }

        readonly IServiceProvider _serviceProvider;

        readonly Type _externalLoginType;

        readonly string _externalLoginName;

        readonly object[] _externalLoginArguments;

        readonly ILinkedUserManager _linkedUserManager;

        /// <summary>
        /// Claim types to be overridden in external claims if local is present.
        /// </summary>
        protected virtual ImmutableHashSet<string> LocalClaimTypes => _localClaimTypes;

        public LinkedLogin(
            LoginDescriptor externalLoginDescriptor,
            IServiceProvider serviceProvider,
            ILinkedUserManager linkedUserManager,
            IUserManager<TUserId> userManager,
            IUsernameFormatter usernameFormatter = null)
            : base(userManager, usernameFormatter)
        {
            if (externalLoginDescriptor == null)
            {
                throw new ArgumentNullException(nameof(externalLoginDescriptor));
            }

            _externalLoginType = externalLoginDescriptor.Type;
            _externalLoginName = externalLoginDescriptor.Name;
            _externalLoginArguments = externalLoginDescriptor.Arguments;
            _linkedUserManager = linkedUserManager ?? throw new ArgumentNullException(nameof(linkedUserManager));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public override async Task<ClaimCollection> LoginAsync(string passcode, CancellationToken cancellationToken = default(CancellationToken))
        {
            var login = (ILogin)ActivatorUtilities.CreateInstance(_serviceProvider, _externalLoginType, _externalLoginArguments);
            try
            {
                var claims = await login.LoginAsync(passcode, cancellationToken).ConfigureAwait(false);
                if (null == claims)
                {
                    return null;
                }
                var id = GetExternalId(claims);
                if (null == id)
                {
                    throw new InvalidOperationException($"Unable to get external id from {claims}.");
                }
                var user = await _linkedUserManager.GetUserAsync(_externalLoginName, id, cancellationToken).ConfigureAwait(false);
                if (null == user)
                {
                    return null;
                }
                if (user is IUser<TUserId> localUser)
                {
                    var localClaims = await GetClaimsAsync(localUser, !claims.HasClaim(c => c.Type == claims.NameClaimType)).ToList(cancellationToken).ConfigureAwait(false);
                    var externalRoleType = claims.RoleClaimType;
                    var externalNameType = claims.NameClaimType;
                    foreach (var claim in claims)
                    {
                        if (claim.Type == externalRoleType)
                        {
                            localClaims.Add(new ClaimDescriptor(ClaimTypes.Role, claim.Value));
                        }
                        else if (claim.Type == externalRoleType)
                        {
                            localClaims.Add(new ClaimDescriptor(ClaimTypes.Name, claim.Value));
                        }
                        else if (!LocalClaimTypes.Contains(claim.Type))
                        {
                            localClaims.Add(claim);
                        }
                    }
                    return new ClaimCollection(localClaims, "login", ClaimTypes.Name, ClaimTypes.Role);
                }
                throw new InvalidOperationException($"User of type {user.GetType().FullName} is uncapable of password authenticaton. IUser<TId> interface must be implemented.");
            }
            finally
            {
                (login as IDisposable)?.Dispose();
            }
        }
    }
}