using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCoreUtils.Linq;

namespace NCoreUtils.Authentication
{
    public class LoginAuthenticator
    {
        readonly IServiceProvider _serviceProvider;

        readonly LoginCollection _loginCollection;

        readonly ILogger _logger;

        public LoginAuthenticator(IServiceProvider serviceProvider, LoginCollection loginCollection, ILogger<LoginAuthenticator> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _loginCollection = loginCollection ?? throw new ArgumentNullException(nameof(loginCollection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IAsyncEnumerable<ClaimCollection> AuthenticateAsync(LoginRequest[] loginRequests)
        {
            var logins = new List<(Type type, object[] args, string loginName, string passcode)>(loginRequests.Length);
            foreach (var loginRequest in loginRequests)
            {
                if (_loginCollection.TryGetValue(loginRequest.LoginName, out var loginDesc))
                {
                    logins.Add((loginDesc.type, loginDesc.args, loginRequest.LoginName, loginRequest.Passcode));
                }
                else
                {
                    _logger.LogWarning("No login registered for name {0}, skipping.", loginRequest.LoginName);
                }
            }
            return logins
                .ToAsyncEnumerable()
                .SelectAsync(async (data, cancellationToken) => {
                    _logger.LogTrace("Authenticating with {0} login.", data.loginName);
                    try
                    {
                        var login = (ILogin)ActivatorUtilities.CreateInstance(_serviceProvider, data.type, data.args);
                        try
                        {
                            var claims = await login.LoginAsync(data.passcode, cancellationToken).ConfigureAwait(false);
                            if (null == claims)
                            {
                                _logger.LogTrace("Authentication failed for {0} login.", data.loginName);
                            }
                            return claims;
                        }
                        finally
                        {
                            (login as IDisposable)?.Dispose();
                        }
                    }
                    catch (Exception exn)
                    {
                        _logger.LogError(exn, "Authentication failed for {0} login due to error.", data.loginName);
                        return null;
                    }
                })
                .Where(claims => null != claims);
        }
    }
}