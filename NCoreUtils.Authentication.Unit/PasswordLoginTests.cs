using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace NCoreUtils.Authentication.Unit
{
    public class PasswordLoginTests
    {
        class User : ILocalUser<int>
        {
            public int Id { get; set; }

            public string Email { get; set; }

            public string FamilyName { get; set; }

            public string GivenName { get; set; }

            public string Password { get; set; }

            public string Salt { get; set; }
        }

        class UserManager : IUserManager<int>
        {
            public List<User> Users { get; } = new List<User>();


            public Task<IUser<int>> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IUser<int>>(Users.FirstOrDefault(u => u.Email == email));
            }

            public IAsyncEnumerable<string> GetPermissionsAsync(IUser<int> user)
            {
                return new string[0].ToAsyncEnumerable();
            }
        }


        [Fact]
        public void SuccessfullAuthentication()
        {
            var userManager = new UserManager();
            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IUserManager<int>>(userManager)
                .AddLoginAuthentication(b => b.AddPasswordAuthentication<UserManager, int>())
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var auth = scope.ServiceProvider.GetRequiredService<ILoginAuthentication>();
                var pwd = PasswordLogin.GeneratePaswordHash("xasd");
                userManager.Users.Add(new User
                {
                    Id = 1,
                    Email = "a@a.test",
                    FamilyName = "a",
                    GivenName = "b",
                    Password = pwd.Hash,
                    Salt = pwd.Salt
                });
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
                var p = auth.AuthenticateAsync("password", "a@a.test:xasd").Result;
                Assert.True(p.Identity.IsAuthenticated);
                Assert.Equal("1", p.FindFirst(ClaimTypes.Sid)?.Value);
                Assert.Equal("a", p.FindFirst(ClaimTypes.Surname)?.Value);
                Assert.Equal("b", p.FindFirst(ClaimTypes.GivenName)?.Value);
                Assert.Equal("b a", p.FindFirst(ClaimTypes.Name)?.Value);
                Assert.Equal("a@a.test", p.FindFirst(ClaimTypes.Email)?.Value);
            }

        }
    }
}