using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Authentication
{
    public interface ILinkedUserManager
    {
        Task<IUser> GetUserAsync(string loginName, string id, CancellationToken cancellationToken = default(CancellationToken));
    }
}