using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Authentication
{
    public interface ILoginAuthentication
    {
        ValueTask<ClaimsPrincipal> AuthenticateAsync(string name, string passcode, CancellationToken cancellationToken = default(CancellationToken));
    }
}