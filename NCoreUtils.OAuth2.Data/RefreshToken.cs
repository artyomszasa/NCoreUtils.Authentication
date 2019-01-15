using NCoreUtils.Data;

namespace NCoreUtils.OAuth2.Data
{
    public class RefreshToken : IHasId<long>, IHasState
    {
        public long Id { get; set; }

        public State State { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public long IssuedAt { get; set; }

        public long ExpiresAt { get; set; }

        public string Scopes { get; set; }

        public long? LastUsed { get; set; }
    }
}