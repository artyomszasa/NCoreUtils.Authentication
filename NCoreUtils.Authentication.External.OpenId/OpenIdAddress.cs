using System.Linq;
using Newtonsoft.Json;

namespace NCoreUtils.Authentication.OpenId
{
    /// <summary>
    /// Represents a physical mailing address. Implementations MAY return only a subset of the fields of an address,
    /// depending upon the information available and the End-User's privacy preferences. For example, the country and
    /// region might be returned without returning more fine-grained address information. (see:
    /// http://openid.net/specs/openid-connect-core-1_0.html#AddressClaim)
    /// </summary>
    public class OpenIdAddress
    {
        /// <summary>
        /// Full mailing address, formatted for display or use on a mailing label. This field MAY contain multiple
        /// lines, separated by newlines. Newlines can be represented either as a carriage return/line feed pair
        /// (<c>"\r\n"</c>) or as a single line feed character (<c>"\n"</c>).
        /// </summary>
        [JsonProperty("formatted")]
        public string Formatted { get; set; }

        /// <summary>
        /// Full street address component, which MAY include house number, street name, Post Office Box, and multi-line
        /// extended street address information. This field MAY contain multiple lines, separated by newlines. Newlines
        /// can be represented either as a carriage return/line feed pair (<c>"\r\n"</c>) or as a single line feed
        /// character (<c>"\n"</c>).
        /// </summary>
        [JsonProperty("street_address")]
        public string StreetAddress { get; set; }

        /// <summary>
        /// City or locality component.
        /// </summary>
        [JsonProperty("locality")]
        public string Locality { get; set; }

        /// <summary>
        /// State, province, prefecture, or region component.
        /// </summary>
        [JsonProperty("region")]
        public string Region { get; set; }

        /// <summary>
        /// Zip code or postal code component.
        /// </summary>
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Country name component.
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Formatted))
            {
                return Formatted;
            }
            var line0 = string.Join(",", new [] { Country, Region }.Where(str => !string.IsNullOrWhiteSpace(str)));
            var line1 = string.Join(",", new [] { PostalCode, Locality }.Where(str => !string.IsNullOrWhiteSpace(str)));

            return string.Join(",", new [] {
                line0,
                line1,
                StreetAddress
            }.Where(str => !string.IsNullOrWhiteSpace(str)));
        }
    }
}