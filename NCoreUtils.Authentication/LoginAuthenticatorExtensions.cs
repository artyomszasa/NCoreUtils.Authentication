using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Authentication
{
    public static class LoginAuthenticatorExtensions
    {
        public static Task<ClaimCollection> AuthenticateAsync(
            this LoginAuthenticator loginAuthenticator,
            LoginRequest loginRequest,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return loginAuthenticator.AuthenticateAsync(new [] { loginRequest })
                .FirstOrDefault(cancellationToken);
        }

        public static Task<ClaimCollection> AuthenticateAsync(
            this LoginAuthenticator loginAuthenticator,
            string loginName,
            string passcode,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return loginAuthenticator.AuthenticateAsync(new LoginRequest(loginName, passcode), cancellationToken);
        }
    }
}