using Microsoft.Extensions.Logging;
using NCoreUtils.Authentication.OAuth2;

namespace NCoreUtils.Authentication.OpenId
{
    [Login("openid")]
    public class OpenIdLogin : OpenIdLogin<OAuth2Configuration<OpenIdLogin>, OAuth2AccessTokenResponse>
    {
        public OpenIdLogin(ILogger<OpenIdLogin> logger, OAuth2Configuration<OpenIdLogin> configuration, IUsernameFormatter usernameFormatter = null)
            : base(logger, configuration, usernameFormatter)
        { }
    }
}