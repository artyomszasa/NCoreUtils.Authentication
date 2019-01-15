using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCoreUtils.Authentication.OAuth2;
using NCoreUtils.Authentication.OpenId;
using Newtonsoft.Json;

namespace NCoreUtils.Authentication
{
    [Login("google")]
    public class GoogleLogin : OpenIdLogin<OAuth2Configuration<GoogleLogin>, OAuth2AccessTokenResponse>
    {
        static readonly IEnumerable<KeyValuePair<string, string>> _emptyParameters = new KeyValuePair<string, string>[0];

        public GoogleLogin(ILogger<GoogleLogin> logger, OAuth2Configuration<GoogleLogin> configuration)
            : base(logger, configuration)
        { }

        protected override IEnumerable<KeyValuePair<string, string>> GetUserInfoRequestParameters(string accessToken)
            => _emptyParameters;

        protected override HttpRequestMessage CreateUserInfoRequestMessage(string accessToken)
        {
            var message = base.CreateUserInfoRequestMessage(accessToken);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return message;
        }

        protected override async Task<string> FormatInvalidAccessTokenResponseMessage(HttpResponseMessage response)
        {
            if (null != response.Content && response.Content.Headers.ContentType.IsJson())
            {
                var json = await response.Content.ReadAsStringAsync();
                try
                {
                    var error = JsonConvert.DeserializeObject<GoogleErrorResponse>(json);
                    return error.ToString();
                }
                catch
                {
                    return $"Server responded with status code {response.StatusCode}, json playload = {GoogleOAuth2LoginHelpers._ws.Replace(json, " ")}.";
                }
            }
            return await base.FormatInvalidAccessTokenResponseMessage(response);
        }
    }
}