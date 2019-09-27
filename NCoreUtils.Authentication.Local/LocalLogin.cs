using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
// using NCoreUtils.Authentication.Internal;

namespace NCoreUtils.Authentication.Local
{
    public abstract class LocalLogin<TUserId> : ILogin
    {

        protected IUserManager<TUserId> UserManager { get; }

        protected IUsernameFormatter UsernameFormatter { get; }

        protected LocalLogin(IUserManager<TUserId> userManager, IUsernameFormatter usernameFormatter = null)
        {
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            UsernameFormatter = usernameFormatter ?? DefaultUsernameFormatter.SharedInstance;
        }

        protected virtual IAsyncEnumerable<ClaimDescriptor> GetClaimsAsync(IUser<TUserId> user, bool forceName = true)
        {
            var name = UsernameFormatter.Format(user);
            var claims = new List<ClaimDescriptor>
            {
                new ClaimDescriptor(ClaimTypes.Sid, user.Id.ToString()),
                new ClaimDescriptor(ClaimTypes.Email, user.Email),
                new ClaimDescriptor(ClaimTypes.GivenName, user.GivenName),
                new ClaimDescriptor(ClaimTypes.Surname, user.FamilyName)
            };
            if (!string.IsNullOrWhiteSpace(name))
            {
                claims.Add(new ClaimDescriptor(ClaimTypes.Name, name));
            }
            else if (forceName)
            {
                claims.Add(new ClaimDescriptor(ClaimTypes.Name, user.Email));
            }
            var roleClaims = UserManager.GetPermissionsAsync(user)
                .Select(permission => new ClaimDescriptor(ClaimTypes.Role, permission));
            return claims.ToAsyncEnumerable().Concat(roleClaims);
        }

        public abstract ValueTask<ClaimCollection> LoginAsync(string passcode, CancellationToken cancellationToken = default(CancellationToken));
    }
}