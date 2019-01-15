using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using NCoreUtils.Authentication.OAuth2;

namespace NCoreUtils.Authentication.OpenId
{
    public abstract class OpenIdLogin<TConfiguration, TAccessTokenResponse> : OAuth2Login<TConfiguration, TAccessTokenResponse, OpenIdUserInfoReponse>
        where TConfiguration : class, IOAuth2Configuration
        where TAccessTokenResponse : class, IOAuth2AccessTokenResponse
    {

        static ClaimDescriptor NotNullClaim(string type, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return new ClaimDescriptor(type, value);
        }

        public OpenIdLogin(ILogger<OpenIdLogin<TConfiguration, TAccessTokenResponse>> logger, TConfiguration configuration, IUsernameFormatter usernameFormatter = null)
            : base(logger, configuration, usernameFormatter)
        { }

        protected override IAsyncEnumerable<ClaimDescriptor> GetClaimsAsync(OpenIdUserInfoReponse user)
        {
            var name = string.IsNullOrWhiteSpace(user.Name) ? UsernameFormatter.Format(user) : user.Name;
            return new ClaimDescriptor []
            {
                new ClaimDescriptor(OpenIdClaimTypes.Sub, user.Sub),
                NotNullClaim(OpenIdClaimTypes.Name, string.IsNullOrWhiteSpace(name) ? user.Email : name),
                NotNullClaim(OpenIdClaimTypes.FamilyName, user.FamilyName),
                NotNullClaim(OpenIdClaimTypes.GivenName, user.GivenName),
                NotNullClaim(OpenIdClaimTypes.MiddleName, user.MiddleName),
                NotNullClaim(OpenIdClaimTypes.Nickname, user.Nickname),
                NotNullClaim(OpenIdClaimTypes.PreferredUsername, user.PreferredUsername),
                NotNullClaim(OpenIdClaimTypes.Profile, user.Profile),
                NotNullClaim(OpenIdClaimTypes.Picture, user.Picture),
                NotNullClaim(OpenIdClaimTypes.Website, user.Website),
                new ClaimDescriptor(OpenIdClaimTypes.Email, user.Email),
                NotNullClaim(OpenIdClaimTypes.Gender, user.Gender),
                NotNullClaim(OpenIdClaimTypes.Birthdate, user.Birthdate),
                NotNullClaim(OpenIdClaimTypes.Zoneinfo, user.Zoneinfo),
                NotNullClaim(OpenIdClaimTypes.Locale, user.Locale),
                NotNullClaim(OpenIdClaimTypes.PhoneNumber, user.PhoneNumber),
                NotNullClaim(OpenIdClaimTypes.UpdatedAt, user.UpdatedAt?.ToString()),
                NotNullClaim(OpenIdClaimTypes.FormattedAddress, user.Address?.Formatted),
                NotNullClaim(OpenIdClaimTypes.StreetAddress, user.Address?.StreetAddress),
                NotNullClaim(OpenIdClaimTypes.Locality, user.Address?.Locality),
                NotNullClaim(OpenIdClaimTypes.Region, user.Address?.Region),
                NotNullClaim(OpenIdClaimTypes.PostalCode, user.Address?.PostalCode),
                NotNullClaim(OpenIdClaimTypes.Country, user.Address?.Country)
            }
                .Where(claim => null != claim)
                .ToAsyncEnumerable();
        }
    }
}