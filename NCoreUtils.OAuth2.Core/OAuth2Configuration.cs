using System;

namespace NCoreUtils.OAuth2
{
    public class OAuth2Configuration
    {
        public TimeSpan AccessTokenExpiry { get; set; } = TimeSpan.FromMinutes(15);

        public TimeSpan AuthorizationCodeExpiry { get; set; } = TimeSpan.FromMinutes(10);

        public TimeSpan RefreshTokenExpiry { get; set; } = TimeSpan.FromDays(30);
    }
}