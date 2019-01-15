using System;
using NCoreUtils.Data;

namespace NCoreUtils.OAuth2.Data
{
    public class AuthorizationCode : IHasId<Guid>
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public long IssuedAt { get; set; }

        public long ExpiresAt { get; set; }

        public string Scopes { get; set; }

        public string RedirectUri { get; set; }
    }
}