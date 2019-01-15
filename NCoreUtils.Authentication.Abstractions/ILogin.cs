using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Authentication
{
    public interface ILogin
    {
        Task<ClaimCollection> LoginAsync(string passcode, CancellationToken cancellationToken = default(CancellationToken));
    }
}