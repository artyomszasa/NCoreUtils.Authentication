using NCoreUtils.Data;

namespace NCoreUtils.OAuth2.Data
{
    public class Domain : IHasId<int>
    {
        public int Id { get; set; }

        public int ClientApplicationId { get; set; }

        public ClientApplication ClientApplication { get; set; }

        public string DomainName { get; set; }
    }
}