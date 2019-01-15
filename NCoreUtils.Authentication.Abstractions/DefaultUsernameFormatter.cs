using System.Globalization;
using System.Linq;

namespace NCoreUtils.Authentication
{
    public class DefaultUsernameFormatter : IUsernameFormatter
    {
        public static DefaultUsernameFormatter SharedInstance { get; } = new DefaultUsernameFormatter();

        public string Format(IUser user)
        {
            if (CultureInfo.CurrentCulture.Name == "hu-HU")
            {
                return string.Join(" ", new []
                {
                    user.FamilyName,
                    user.GivenName
                }.Where(str => !string.IsNullOrEmpty(str)));
            }
            return string.Join(" ", new []
            {
                user.GivenName,
                user.FamilyName
            }.Where(str => !string.IsNullOrEmpty(str)));
        }
    }
}