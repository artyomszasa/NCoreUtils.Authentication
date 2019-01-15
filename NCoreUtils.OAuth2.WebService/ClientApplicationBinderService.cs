using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCoreUtils.Data;
using NCoreUtils.Linq;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2.WebService
{
    class ClientApplicationBinderService
    {
        readonly object _sync = new object();

        readonly IDataRepository<ClientApplication> _clientApplicationRepository;

        readonly IHttpContextAccessor _httpContextAccessor;

        readonly ILogger _logger;

        Task<ClientApplication> _task;

        public ClientApplicationBinderService(
            IDataRepository<ClientApplication> clientApplicationRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ClientApplicationBinderService> logger)
        {
            _clientApplicationRepository = clientApplicationRepository ?? throw new ArgumentNullException(nameof(clientApplicationRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        async Task<ClientApplication> DoResolveClientApplication(CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (null == httpContext)
            {
                _logger.LogError("Unable to access http context.");
                return null;
            }
            var request = httpContext.Request;
            if (!request.Host.HasValue)
            {
                _logger.LogDebug("Unable to resolve client application: No Host value.");
                return null;
            }
            var host = request.Host.Value;
            var clientApplication = await _clientApplicationRepository.Items.FirstOrDefaultAsync(ca => ca.Domains.Any(domain => domain.DomainName == host), cancellationToken);
            if (null == clientApplication)
            {
                _logger.LogDebug("Unable to resolve client application: No client application found for {0}.", host);
                return null;
            }
            _logger.LogDebug("Successfully resolved client application for {0} => {1}.", host, clientApplication.Name);
            return clientApplication;
        }

        public Task<ClientApplication> ResolveClientApplication(CancellationToken cancellationToken)
        {
            if (null == _task)
            {
                lock (_sync)
                {
                    if (null == _task)
                    {
                        _task = DoResolveClientApplication(cancellationToken);
                    }
                }
            }
            return _task;
        }
    }
}