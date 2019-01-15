using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NCoreUtils.Authentication;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2.WebService
{
    class OAuth2PasswordLogin : PasswordLogin<int>
    {
        public OAuth2PasswordLogin(
            OAuth2UserManager userManager,
            ILogger<OAuth2PasswordLogin> logger,
            PasswordLoginOptions options = null,
            IUsernameFormatter usernameFormatter = null)
            : base(userManager, logger, options, usernameFormatter)
        { }

        protected override IAsyncEnumerable<ClaimDescriptor> GetClaimsAsync(IUser<int> user, bool forceName = true)
        {
            var baseClaims = base.GetClaimsAsync(user, forceName);
            if (user is User u)
            {
                ClaimDescriptor[] clientApplicationClaims;
                if (null != u.ClientApplication)
                {
                    clientApplicationClaims = new []
                    {
                        new ClaimDescriptor(OAuth2ClientApplicationClaims.ClientApplicationId, u.ClientApplictionId.ToString()),
                        new ClaimDescriptor(OAuth2ClientApplicationClaims.ClientApplicationName, u.ClientApplication.Name)
                    };
                }
                else
                {
                    clientApplicationClaims = new []
                    {
                        new ClaimDescriptor(OAuth2ClientApplicationClaims.ClientApplicationId, u.ClientApplictionId.ToString())
                    };
                }
                return baseClaims.Concat(clientApplicationClaims.ToAsyncEnumerable());
            }
            return baseClaims;
        }
    }
}