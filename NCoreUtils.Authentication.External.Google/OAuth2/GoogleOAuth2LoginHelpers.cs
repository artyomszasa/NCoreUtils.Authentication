using System.Text.RegularExpressions;

namespace NCoreUtils.Authentication.OAuth2
{
    static class GoogleOAuth2LoginHelpers
    {
        public static readonly Regex _ws = new Regex("[\t\n\r]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }
}