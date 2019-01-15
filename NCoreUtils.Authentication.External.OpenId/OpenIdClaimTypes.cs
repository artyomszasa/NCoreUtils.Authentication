using System.Security.Claims;

namespace NCoreUtils.Authentication.OpenId
{
    public static class OpenIdClaimTypes
    {
        public const string Sub = "openid/sub";

        public const string Name = ClaimTypes.Name;

        public const string GivenName = ClaimTypes.GivenName;

        public const string FamilyName = ClaimTypes.Surname;

        public const string MiddleName = "openid/middle_name";

        public const string Nickname = "openid/nickname";

        public const string PreferredUsername = "openid/preferred_username";

        public const string Profile = ClaimTypes.Uri;

        public const string Picture = "openid/picture";

        public const string Website = ClaimTypes.Webpage;

        public const string Email = ClaimTypes.Email;

        public const string Gender = ClaimTypes.Gender;

        public const string Birthdate = ClaimTypes.DateOfBirth;

        public const string Zoneinfo = "openid/zoneinfo";

        public const string Locale = "openid/locale";

        public const string PhoneNumber = ClaimTypes.OtherPhone;

        public const string UpdatedAt = "openid/updated_at";

        public const string FormattedAddress = "openid/address/formatted";

        public const string StreetAddress = ClaimTypes.StreetAddress;

        public const string Locality = ClaimTypes.Locality;

        public const string Region = ClaimTypes.StateOrProvince;

        public const string PostalCode = ClaimTypes.PostalCode;

        public const string Country = ClaimTypes.Country;
    }
}