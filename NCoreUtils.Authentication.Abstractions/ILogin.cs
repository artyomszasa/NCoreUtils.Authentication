using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Authentication
{
    public interface ILogin
    {
        ValueTask<ClaimCollection> LoginAsync(string passcode, CancellationToken cancellationToken = default(CancellationToken));
    }
}