using Newtonsoft.Json;

namespace NCoreUtils.Authentication.OAuth2
{
    public class OAuth2AccessTokenResponse : IOAuth2AccessTokenResponse
    {
        [JsonProperty("access_token", Required = Required.Always)]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in", Required = Required.Always)]
        public long ExpiresIn { get; set; }

        [JsonProperty("token_type", Required = Required.Always)]
        public string TokenType { get; set; }
    }
}