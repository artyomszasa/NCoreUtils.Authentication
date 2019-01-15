using System.Collections.Generic;
using NCoreUtils.Data;

namespace NCoreUtils.OAuth2.Data
{
    public class Permission : IHasId<int>
    {
        public int Id { get; set; }

        public int ClientApplictionId { get; set; }

        public ClientApplication ClientApplication { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<UserPermission> Users { get; set; } = new HashSet<UserPermission>();
    }
}