using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Authentication;
using NCoreUtils.Data;
using NCoreUtils.Linq;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2.WebService
{
    public class OAuth2UserManager : IUserManager<int>
    {
        readonly IDataRepository<User> _userRepository;

        readonly CurrentClientApplication _currentClientApplication;

        public OAuth2UserManager(IDataRepository<User> userRepository, CurrentClientApplication currentClientApplication)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _currentClientApplication = currentClientApplication ?? throw new ArgumentNullException(nameof(currentClientApplication));
        }

        public async Task<IUser<int>> FindByEmailAsync(string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            var appId = _currentClientApplication.Id;
            return await _userRepository.Items.Where(u => u.Email == email && u.ClientApplictionId == appId).FirstOrDefaultAsync(cancellationToken);
        }

        public IAsyncEnumerable<string> GetPermissionsAsync(IUser<int> user)
        {
            var userId = user.Id;
            return _userRepository.Items
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Permissions)
                .Select(up => up.Permission.Name)
                .ExecuteAsync();
        }
    }
}