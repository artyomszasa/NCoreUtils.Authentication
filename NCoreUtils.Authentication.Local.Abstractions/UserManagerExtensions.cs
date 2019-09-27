using System.Collections.Generic;
using System.Linq;

namespace NCoreUtils.Authentication
{
    public static class UserManagerExtensions
    {
        public static IUser<TUserId> FindByEmail<TUserId>(this IUserManager<TUserId> userManager, string email)
            => userManager.FindByEmailAsync(email).GetAwaiter().GetResult();

        public static IEnumerable<string> GetPermissions<TUserId>(this IUserManager<TUserId> userManager, IUser<TUserId> user)
        {
            var enumerable = userManager.GetPermissionsAsync(user);
            var enumerator = enumerable.GetAsyncEnumerator();
            try
            {
                while (enumerator.MoveNextAsync().AsTask().Result)
                {
                    yield return enumerator.Current;
                }
            }
            finally
            {
                enumerator.DisposeAsync().AsTask().Wait();
            }
        }
            // => userManager.GetPermissionsAsync(user).ToEnumerable();
    }
}