using System.Collections.Generic;
using NCoreUtils.Authentication;
using NCoreUtils.Data;

namespace NCoreUtils.OAuth2.Data
{
    public class User : IHasId<int>, ILocalUser<int>, IHasState, IHasTimeTracking
    {
        public int Id { get; set; }

        public long Created { get; set; }

        public long Updated { get; set; }

        public State State { get; set; }

        public int ClientApplictionId { get; set; }

        public ClientApplication ClientApplication { get; set; }

        public string HonorificPrefix { get; set; }

        public string FamilyName { get; set; }

        public string GivenName { get; set; }

        public string MiddleName { get; set; }

        public string Email { get; set; }

        public string Salt { get; set; }

        public string Password { get; set; }

        public ICollection<UserPermission> Permissions { get; set; } = new HashSet<UserPermission>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();

        public ICollection<AuthorizationCode> AuthorizationCodes { get; set; } = new HashSet<AuthorizationCode>();
    }
}