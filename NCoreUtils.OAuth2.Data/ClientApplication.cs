using System.Collections.Generic;
using NCoreUtils.Data;

namespace NCoreUtils.OAuth2.Data
{
    public class ClientApplication : IHasId<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Domain> Domains { get; set; } = new HashSet<Domain>();

        public ICollection<User> Users { get; set; } = new HashSet<User>();

        public ICollection<Permission> Permissions { get; set; } = new HashSet<Permission>();
    }
}