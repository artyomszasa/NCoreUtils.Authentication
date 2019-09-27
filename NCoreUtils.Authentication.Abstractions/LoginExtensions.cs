using System.Security.Claims;

namespace NCoreUtils.Authentication
{
    public static class LoginExtensions
    {
        public static ClaimCollection Login(this ILogin login, string passcode)
        {
            var res = login.LoginAsync(passcode);
            if (res.IsCompletedSuccessfully)
            {
                return res.Result;
            }
            return res.AsTask().GetAwaiter().GetResult();
        }
    }
}