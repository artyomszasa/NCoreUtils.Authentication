using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
// using NCoreUtils.Authentication.Internal;
using NCoreUtils.Authentication.Local;

namespace NCoreUtils.Authentication
{
    [Login("password")]
    public class PasswordLogin<TUserId> : LocalLogin<TUserId>
    {
        static bool TryExtractParameters(string passcode, out string email, out string password)
        {
            if (!string.IsNullOrWhiteSpace(passcode))
            {
                var index = passcode.IndexOf(':');
                if (-1 != index)
                {
                    email = passcode.Substring(0, index);
                    password = passcode.Substring(index + 1);
                    return true;
                }
            }
            email = default(string);
            password = default(string);
            return false;
        }

        readonly ILogger _logger;

        readonly PasswordLoginOptions _options;

        public PasswordLogin(IUserManager<TUserId> userManager, ILogger<PasswordLogin<TUserId>> logger, PasswordLoginOptions options = null, IUsernameFormatter usernameFormatter = null)
            : base(userManager, usernameFormatter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? PasswordLoginOptions.Default;
        }

        public override async ValueTask<ClaimCollection> LoginAsync(string passcode, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!TryExtractParameters(passcode, out var email, out var password))
            {
                _logger.LogDebug("Invalid passcode for password authentication. Should be a non-empty string in the following format: email:password.");
                return null;
            }
            if (_options.IsSensitiveLoggingEnabled)
            {
                _logger.LogDebug("Attempting password login with email = \"{0}\" and password = \"{1}\".", email, password);
            }
            else
            {
                _logger.LogDebug("Attempting password login with email = \"{0}\".", email);
            }
            var user = await UserManager.FindByEmailAsync(email, cancellationToken).ConfigureAwait(false);
            if (null == user)
            {
                _logger.LogInformation("Failed attempt to login with email = \"{0}\": no such user.", email);
                return null;
            }
            if (user is ILocalUser<TUserId> localUser)
            {
                if (PasswordLogin.CheckPasswordHash(localUser, password))
                {
                    var claims = await GetClaimsAsync(localUser).ToListAsync(cancellationToken).ConfigureAwait(false);
                    return new ClaimCollection(claims, "login", ClaimTypes.Role, ClaimTypes.Name);
                }
                if (_options.IsSensitiveLoggingEnabled)
                {
                    _logger.LogInformation("Failed attempt to login with email = \"{0}\": invalid password = \"{1}\".", email, password);
                }
                else
                {
                    _logger.LogInformation("Failed attempt to login with email = \"{0}\": invalid password.", email);
                }
                return null;
            }
            throw new InvalidOperationException($"User of type {user.GetType().FullName} is uncapable of password authenticaton. ILocalUser interface must be implemented.");
        }
    }
}