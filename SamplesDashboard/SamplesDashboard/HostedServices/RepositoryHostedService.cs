// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using SamplesDashboard.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace SamplesDashboard.HostedServices
{
    public class RepositoryHostedService : IHostedService, IDisposable
    {
        private readonly RepositoriesService _repositoryService;
        private readonly ILogger<RepositoryHostedService> _logger;
        private Timer _timer;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public RepositoryHostedService(RepositoriesService repositoryService, ILogger<RepositoryHostedService> logger, IMemoryCache cache, IConfiguration config)
        {
            _repositoryService = repositoryService;
            _logger = logger;
            _cache = cache;
            _config = config;
        }

        /// <summary>
        /// service cleanup
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();    
        }

        /// <summary>
        /// Handler method for service starting
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>completed task</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async state => await CheckCacheForRepositories(), null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            return Task.CompletedTask;
        }

        /// <summary>
        ///Handler method for service stopping
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>completed task</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(RepositoryHostedService)} is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// checks state of cache
        /// </summary>
        /// <returns></returns>
        private async Task CheckCacheForRepositories()
        {
            if (!_cache.TryGetValue(Constants.Samples, out var samples))
            {
                samples = await _repositoryService.GetRepositories(Constants.Samples);

                //Read timeout from config file 
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));

                // Save data in cache.
                _cache.Set(Constants.Samples, samples, cacheEntryOptions);
                _logger.LogInformation($"{nameof(RepositoryHostedService)} :samples cache refreshed");

            }

            if (!_cache.TryGetValue(Constants.Sdks, out  var sdkList))
            {
                sdkList = await _repositoryService.GetRepositories(Constants.Sdks);

                //Read timeout from config file 
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));

                // Save data in cache.
                _cache.Set(Constants.Sdks, sdkList, cacheEntryOptions);
                _logger.LogInformation($"{nameof(RepositoryHostedService)} :sdks cache refreshed");

            }
        }

    }
}
