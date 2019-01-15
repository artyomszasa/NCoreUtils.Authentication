using Newtonsoft.Json;

namespace NCoreUtils.OAuth2.WebService.ViewModel
{
    public class OAuth2Error
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}