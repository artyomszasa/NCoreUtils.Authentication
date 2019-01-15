using Newtonsoft.Json;

namespace NCoreUtils.OAuth2.WebService.ViewModel
{
    public class OpenIDUserInfo
    {
        /// <summary>
        /// Felhasználó azonosítója.
        /// </summary>
        [JsonProperty("sub")]
        public int Id { get; set; }

        /// <summary>
        /// Keresztnév
        /// </summary>
        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        /// <summary>
        /// Családnév
        /// </summary>
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }

        /// <summary>
        /// Kép
        /// </summary>
        [JsonProperty("picture")]
        public string Picture { get; set; }

        /// <summary>
        /// Email cím
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Preferált lokalizáció.
        /// </summary>
        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("scope")]
        public string[] Scopes { get; set; }

    }
}