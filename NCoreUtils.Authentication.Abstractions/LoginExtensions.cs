using System.Security.Claims;

namespace NCoreUtils.Authentication
{
    public static class LoginExtensions
    {
        public static ClaimCollection Login(this ILogin login, string passcode)
            => login.LoginAsync(passcode).GetAwaiter().GetResult();
    }
}