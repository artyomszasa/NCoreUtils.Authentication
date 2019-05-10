using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Authentication
{
    public class DefaultUsernameFormatter : IUsernameFormatter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string JoinNonEmpty(string a, string b)
        {
            string result;
            if (string.IsNullOrEmpty(a))
            {
                if (string.IsNullOrEmpty(b))
                {
                    result = string.Empty;
                }
                else
                {
                    result = b;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(b))
                {
                    result = a;
                }
                else
                {
                    result = a + ' ' + b;
                }
            }
            return result;
        }

        public static DefaultUsernameFormatter SharedInstance { get; } = new DefaultUsernameFormatter();

        public string Format(IUser user)
        {
            if (CultureInfo.CurrentCulture.Name == "hu-HU")
            {
                return JoinNonEmpty(user.FamilyName, user.GivenName);
            }
            return JoinNonEmpty(user.GivenName, user.FamilyName);
        }
    }
}