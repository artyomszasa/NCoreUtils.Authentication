using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;

namespace NCoreUtils.Authentication
{
    [Serializable]
    public sealed class ClaimCollection : IReadOnlyCollection<ClaimDescriptor>, ISerializable
    {
        static IEnumerable<ClaimDescriptor> GetClaims(SerializationInfo info, StreamingContext context)
        {
            var count = info.GetInt32("ClaimCount");
            for (var i = 0; i < count; ++i)
            {
                yield return (ClaimDescriptor)info.GetValue($"Claim{i}", typeof(ClaimDescriptor));
            }
        }

        readonly HashSet<ClaimDescriptor> _claims;

        public string AuthenticationType { get; }

        public string RoleClaimType { get; }

        public string NameClaimType { get; }

        public int Count => _claims.Count;

        internal ClaimCollection(SerializationInfo info, StreamingContext context)
            : this(GetClaims(info, context), info.GetString(nameof(AuthenticationType)), info.GetString(nameof(RoleClaimType)), info.GetString(nameof(NameClaimType)))
        { }

        public ClaimCollection(IEnumerable<ClaimDescriptor> claims, string authenticationType = null, string roleClaimType = ClaimTypes.Role, string nameClaimType = ClaimTypes.Name)
        {
            _claims = null == claims ? new HashSet<ClaimDescriptor>() : new HashSet<ClaimDescriptor>(claims);
            AuthenticationType = authenticationType;
            RoleClaimType = roleClaimType ?? ClaimTypes.Role;
            NameClaimType = nameClaimType ?? ClaimTypes.Name;
        }

        public ClaimCollection(ClaimDescriptor[] claims, string authenticationType = null, string roleClaimType = ClaimTypes.Role, string nameClaimType = ClaimTypes.Name)
            : this((IEnumerable<ClaimDescriptor>)claims, authenticationType, roleClaimType, nameClaimType)
        { }

        public IEnumerator<ClaimDescriptor> GetEnumerator() => _claims.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(AuthenticationType), AuthenticationType);
            info.AddValue(nameof(RoleClaimType), RoleClaimType);
            info.AddValue(nameof(NameClaimType), NameClaimType);
            info.AddValue("ClaimCount", _claims.Count);
            var i = 0;
            foreach (var claim in _claims)
            {
                info.AddValue($"Claim{i}", claim, typeof(ClaimDescriptor));
                ++i;
            }
        }

        public bool HasClaim(Func<ClaimDescriptor, bool> predicate) => _claims.Any(predicate);
    }
}