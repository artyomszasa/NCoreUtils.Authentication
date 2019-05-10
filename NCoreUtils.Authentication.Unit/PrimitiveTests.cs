using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NCoreUtils.Authentication.Unit
{
    public class PrimitiveTests
    {
        class User : IUser
        {
            public string Email { get; set; }

            public string FamilyName { get; set; }

            public string GivenName { get; set; }
        }

        class DummyLogin : ILogin
        {
            public bool Called { get; private set; }

            public Task<ClaimCollection> LoginAsync(string passcode, CancellationToken cancellationToken = default)
            {
                Called = true;
                return Task.FromResult(new ClaimCollection(null));
            }
        }

        static readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        static byte[] Serialize(object @object)
        {
            using (var buffer = new MemoryStream())
            {
                _binaryFormatter.Serialize(buffer, @object);
                return buffer.ToArray();
            }
        }

        static object Deserialize(byte[] data)
        {
            using (var buffer = new MemoryStream(data, false))
            {
                return _binaryFormatter.Deserialize(buffer);
            }
        }

        [Fact]
        public void AuthenticationExceptionTest()
        {
            {
                var exn0 = new AuthenticationException("message");
                var bin = Serialize(exn0);
                var exn = Assert.IsType<AuthenticationException>(Deserialize(bin));
                Assert.Equal(exn0.Message, exn.Message);
            }
            {
                var exn0 = new AuthenticationException("message", new Exception("inner"));
                var bin = Serialize(exn0);
                var exn = Assert.IsType<AuthenticationException>(Deserialize(bin));
                Assert.Equal(exn0.Message, exn.Message);
                Assert.NotNull(exn.InnerException);
                Assert.Equal(exn0.InnerException.Message, exn.InnerException.Message);
            }
        }

        [Theory]
        [InlineData(new object[] { "type", null, null, null, null })]
        [InlineData(new object[] { "type", "value", null, null, null })]
        [InlineData(new object[] { "type", "value", "valueType", null, null })]
        [InlineData(new object[] { "type", "value", "valueType", "issuer", null })]
        [InlineData(new object[] { "type", "value", "valueType", "issuer", "originalIssuer" })]
        public void EqualClaimDescriptorTest(string type, string value, string valueType, string issuer, string originalIssuer)
        {
            var a = new ClaimDescriptor(type, value, valueType, issuer, originalIssuer);
            var b = new ClaimDescriptor(type, value, valueType, issuer, originalIssuer);
            Assert.Equal(type, a.Type);
            Assert.Equal(value, a.Value);
            Assert.Equal(valueType, a.ValueType);
            Assert.Equal(issuer, a.Issuer);
            Assert.Equal(originalIssuer, a.OriginalIssuer);
            Assert.Equal(a, a);
            Assert.Equal(a, b);
            Assert.True(((object)a).Equals(b));
            Assert.False(((object)a).Equals(null));
            Assert.False(((object)a).Equals(2));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ClaimDescriptorTest()
        {
            var exn = Assert.Throws<ArgumentNullException>(() => new ClaimDescriptor(null, null));
            Assert.Equal("type", exn.ParamName);
            Assert.NotEqual(new ClaimDescriptor("x", "b", "c", "d", "e"), new ClaimDescriptor("a", "b", "c", "d", "e"));
            Assert.NotEqual(new ClaimDescriptor("a", "x", "c", "d", "e"), new ClaimDescriptor("a", "b", "c", "d", "e"));
            Assert.NotEqual(new ClaimDescriptor("a", "b", "x", "d", "e"), new ClaimDescriptor("a", "b", "c", "d", "e"));
            Assert.NotEqual(new ClaimDescriptor("a", "b", "c", "x", "e"), new ClaimDescriptor("a", "b", "c", "d", "e"));
            Assert.NotEqual(new ClaimDescriptor("a", "b", "c", "d", "x"), new ClaimDescriptor("a", "b", "c", "d", "e"));
            Assert.NotEqual(new ClaimDescriptor("a", null, "c", "d", "e"), new ClaimDescriptor("a", "b", "c", "d", "e"));
            Assert.NotEqual(new ClaimDescriptor("a", "b", null, "d", "e"), new ClaimDescriptor("a", "b", "c", "d", "e"));
            Assert.NotEqual(new ClaimDescriptor("a", "b", "c", null, "e"), new ClaimDescriptor("a", "b", "c", "d", "e"));
            Assert.NotEqual(new ClaimDescriptor("a", "b", "c", "d", null), new ClaimDescriptor("a", "b", "c", "d", "e"));
            var desc = new ClaimDescriptor("a", "b", "c", "d", "e");
            var descx = new ClaimDescriptor("a", "b", "c", "d", "e");
            Assert.Equal(desc, Deserialize(Serialize(desc)));
            exn = Assert.Throws<ArgumentNullException>(() => desc.Update(null));
            Assert.Equal("update", exn.ParamName);
            var updated = desc.Update(b => b.Value = "x");
            Assert.NotSame(desc, updated);
            Assert.NotEqual(desc, updated);
            Assert.Equal("x", updated.Value);
            Assert.True(desc == descx);
            Assert.False((ClaimDescriptor)null == desc);
            Assert.False(desc == (ClaimDescriptor)null);
            Assert.True((ClaimDescriptor)null == (ClaimDescriptor)null);
            Assert.False(desc != descx);
            Assert.True((ClaimDescriptor)null != desc);
            Assert.True(desc != (ClaimDescriptor)null);
            Assert.False((ClaimDescriptor)null != (ClaimDescriptor)null);

            exn = Assert.Throws<ArgumentNullException>(() => new ClaimDescriptorBuilder().Build());
            Assert.Equal("type", exn.ParamName);
        }

        [Fact]
        public void ClaimCollectionTest()
        {
            var a = new ClaimDescriptor("a", "b", "c", "d", "e");
            var b = new ClaimDescriptor("a", "b", "c", "d", "e");
            var c = new ClaimDescriptor("x", "b", "c", "d", "e");
            var collection = new ClaimCollection(null, null, null, null);
            Assert.Equal(ClaimTypes.Name, collection.NameClaimType);
            Assert.Equal(ClaimTypes.Role, collection.RoleClaimType);
            Assert.Null(collection.AuthenticationType);
            var count = collection.Count;
            Assert.Equal(0, count);
            Assert.True(HashSet<ClaimDescriptor>.CreateSetComparer().Equals(new HashSet<ClaimDescriptor>(collection), new HashSet<ClaimDescriptor>()));
            Assert.True(HashSet<ClaimDescriptor>.CreateSetComparer().Equals(
                new HashSet<ClaimDescriptor>(collection),
                new HashSet<ClaimDescriptor>(Assert.IsType<ClaimCollection>(Deserialize(Serialize(collection))))
            ));
            collection = new ClaimCollection(new [] { a, c });
            Assert.Contains(a, collection);
            Assert.Contains(c, collection);
            Assert.Equal(ClaimTypes.Name, collection.NameClaimType);
            Assert.Equal(ClaimTypes.Role, collection.RoleClaimType);
            Assert.Equal(2, collection.Count);
            Assert.True(HashSet<ClaimDescriptor>.CreateSetComparer().Equals(new HashSet<ClaimDescriptor>(collection), new HashSet<ClaimDescriptor>(new [] { a, c })));

            Assert.True(HashSet<ClaimDescriptor>.CreateSetComparer().Equals(
                new HashSet<ClaimDescriptor>(collection),
                new HashSet<ClaimDescriptor>(Assert.IsType<ClaimCollection>(Deserialize(Serialize(collection))))
            ));

            collection = new ClaimCollection(new [] { a, b, c });
            Assert.Contains(a, collection);
            Assert.Contains(c, collection);
            Assert.Equal(ClaimTypes.Name, collection.NameClaimType);
            Assert.Equal(ClaimTypes.Role, collection.RoleClaimType);
            Assert.Equal(2, collection.Count);
            Assert.True(HashSet<ClaimDescriptor>.CreateSetComparer().Equals(new HashSet<ClaimDescriptor>(collection), new HashSet<ClaimDescriptor>(new [] { a, c })));

            Assert.True(collection.HasClaim(x => x.Equals(a)));
            Assert.True(collection.HasClaim(x => x.Equals(b)));
        }

        [Fact]
        public void DefaultUserNameFormatterTest()
        {
            var f = DefaultUsernameFormatter.SharedInstance;
            CultureInfo.CurrentCulture = new CultureInfo("hu-HU");
            Assert.Equal(string.Empty, f.Format(new User()));
            Assert.Equal("a", f.Format(new User { FamilyName = "a" }));
            Assert.Equal("a", f.Format(new User { GivenName = "a" }));
            Assert.Equal("a b", f.Format(new User { FamilyName = "a", GivenName = "b" }));
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            Assert.Equal(string.Empty, f.Format(new User()));
            Assert.Equal("a", f.Format(new User { FamilyName = "a" }));
            Assert.Equal("a", f.Format(new User { GivenName = "a" }));
            Assert.Equal("b a", f.Format(new User { FamilyName = "a", GivenName = "b" }));
        }

        [Fact]
        public void LoginAttributeTest()
        {
            Assert.Throws<ArgumentException>(() => new LoginAttribute(null));
            Assert.Equal("a", new LoginAttribute("a").Name);
        }

        [Fact]
        public void LoginExtensionsTest()
        {
            var login = new DummyLogin();
            login.Login("xxx");
            Assert.True(login.Called);
        }

        [Fact]
        public void LoginCollectionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new LoginCollection(null));
            {
                var collection = new LoginCollection(ImmutableDictionary<string, (Type, object[])>.Empty);
                Assert.Throws<KeyNotFoundException>(() => collection["x"]);
                Assert.Empty(collection.Keys);
                Assert.Empty(collection.Values);
                var count = collection.Count;
                Assert.Equal(0, count);
                Assert.False(collection.ContainsKey("x"));
                Assert.False(collection.TryGetValue("x", out var _));
            }
            {
                var v = (typeof(int), new object[0]);
                var collection = new LoginCollection(ImmutableDictionary.CreateRange<string, (Type, object[])>(new []
                {
                    new KeyValuePair<string, (Type, object[])> ("x", v)
                }));
                Assert.Equal(v, collection["x"]);
                Assert.True(Enumerable.SequenceEqual(new [] { v }, collection.Select(kv => kv.Value).ToArray()));
                Assert.Throws<KeyNotFoundException>(() => collection["y"]);
                Assert.Single(collection.Keys, "x");
                Assert.Single(collection.Values, v);
                var count = collection.Count;
                Assert.Equal(1, count);
                Assert.True(collection.ContainsKey("x"));
                Assert.True(collection.TryGetValue("x", out var v1));
                Assert.Equal(v, v1);
            }
        }

        [Fact]
        public void LoginRequestTests()
        {
            var a = new LoginRequest();
            var b = new LoginRequest(null, null);
            var c = new LoginRequest("x", "y");
            var d = new LoginRequest("x", "y");
            var e = new LoginRequest("X", "y");
            var f = new LoginRequest("a", "b");
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.Equal(c, d);
            Assert.Equal(c.GetHashCode(), d.GetHashCode());
            Assert.Equal(c, e);
            Assert.Equal(c.GetHashCode(), e.GetHashCode());
            Assert.NotEqual(a, c);
            Assert.NotEqual(c, f);
            Assert.True(((object)c).Equals(d));
            Assert.False(((object)c).Equals(null));
            Assert.False(((object)c).Equals(2));

            Assert.True(a == b);
            Assert.True(c == d);
            Assert.True(c == e);
            Assert.False(a == c);
            Assert.False(c == f);


            Assert.False(a != b);
            Assert.False(c != d);
            Assert.False(c != e);
            Assert.True(a != c);
            Assert.True(c != f);
        }
    }
}