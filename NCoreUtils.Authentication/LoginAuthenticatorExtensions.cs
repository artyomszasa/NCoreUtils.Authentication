using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// using NCoreUtils.Authentication.Internal;

namespace NCoreUtils.Authentication
{
    public static class LoginAuthenticatorExtensions
    {
        public static ValueTask<ClaimCollection> AuthenticateAsync(
            this LoginAuthenticator loginAuthenticator,
            LoginRequest loginRequest,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return loginAuthenticator.AuthenticateAsync(new [] { loginRequest })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public static ValueTask<ClaimCollection> AuthenticateAsync(
            this LoginAuthenticator loginAuthenticator,
            string loginName,
            string passcode,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return loginAuthenticator.AuthenticateAsync(new LoginRequest(loginName, passcode), cancellationToken);
        }
    }
}