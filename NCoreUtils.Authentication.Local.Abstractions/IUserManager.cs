using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Authentication
{
    public interface IUserManager<TUserId>
    {
        Task<IUser<TUserId>> FindByEmailAsync(string email, CancellationToken cancellationToken = default(CancellationToken));

        IAsyncEnumerable<string> GetPermissionsAsync(IUser<TUserId> user);

    }
}