// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This service is run by the server in the background
    /// to pre-load repositories into the in-memory cache
    /// </summary>
    public class RepositoriesHostedService : IHostedService, IDisposable
    {
        private readonly RepositoriesService _repositoriesService;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RepositoriesHostedService> _logger;
        private Timer _timer;

        public RepositoriesHostedService(
            RepositoriesService repositoriesService,
            IConfiguration config,
            IMemoryCache memoryCache,
            ILogger<RepositoriesHostedService> logger
        )
        {
            _repositoriesService = repositoriesService;
            _config = config;
            _cache = memoryCache;
            _logger = logger;
        }

        /// <summary>
        /// Service cleanup
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }

        /// <summary>
        /// Start the background task to check the cache and load repos if needed
        /// </summary>
        /// <param name="cancellationToken">The cancellation token provided by the caller</param>
        /// <returns>Completed task</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(RepositoriesHostedService)} is starting.");
            // Create the time to check cache
            // Delay 1 minute before starting
            // Run every 10 minutes
            _timer = new Timer(async state => await CheckCacheForRepositories(),
                null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop the background task
        /// </summary>
        /// <param name="cancellationToken">The cancellation token provided by the caller</param>
        /// <returns>Completed task</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(RepositoriesHostedService)} is stopping.");
            // Set due time to infinite to prevent timer from restarting
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Calls the RepositoriesService to pre-populate the cache
        /// </summary>
        private async Task CheckCacheForRepositories()
        {
            try
            {
                var cacheLifeTime = _config.GetValue<double>(Constants.CacheLifetime);

                // Populate the samples cache if needed
                if (!_cache.TryGetValue(Constants.Samples, out var samples))
                {
                    var stopWatch = Stopwatch.StartNew();

                    var sampleRepositories = await _repositoriesService.GetRepositoriesAsync(Constants.Samples);
                    samples = sampleRepositories?.Count ?? 0;

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheLifeTime));

                    // Add a cache entry with the number of sample repos to indicate
                    // that the samples have been cached
                    _cache.Set(Constants.Samples, samples, cacheEntryOptions);
                    stopWatch.Stop();
                    _logger.LogInformation($"{nameof(RepositoriesHostedService)}: samples cache refreshed in {stopWatch.ElapsedMilliseconds} milliseconds.");
                }

                // Populate the SDKs cache if needed
                if (!_cache.TryGetValue(Constants.Sdks, out var sdks))
                {
                    var stopWatch = Stopwatch.StartNew();

                    var sdkRepositories = await _repositoriesService.GetRepositoriesAsync(Constants.Sdks);
                    sdks = sdkRepositories?.Count ?? 0;

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheLifeTime));

                    // Add a cache entry with the number of SDK repos to indicate
                    // that the SDKs have been cached
                    _cache.Set(Constants.Sdks, sdks, cacheEntryOptions);
                    stopWatch.Stop();
                    _logger.LogInformation($"{nameof(RepositoriesHostedService)}: SDKs cache refreshed in {stopWatch.ElapsedMilliseconds} milliseconds.");
                }
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, ex.Message);
            }
        }
    }
}
