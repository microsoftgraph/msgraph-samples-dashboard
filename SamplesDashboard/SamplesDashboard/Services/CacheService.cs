// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class manages all cache access
    /// </summary>
    public class CacheService
    {
        private readonly IMemoryCache _cache;
        private readonly double _cacheLifetime;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<CacheService> logger)
        {
            _cache = memoryCache;
            _cacheLifetime = configuration.GetValue<double>(Constants.CacheLifetime);
            _logger = logger;
        }

        /// <summary>
        /// Tries to get a value from the IMemoryCache
        /// </summary>
        /// <param name="key">The key that identifies the value in the cache</param>
        /// <param name="value">The located value or null</param>
        /// <returns>true if item was found, false if not</returns>
        public bool TryGetValue<T>(string key, out T value)
        {
            try
            {
                return _cache.TryGetValue(key, out value);
            }
            catch (ObjectDisposedException odEx)
            {
                _logger.LogWarning(odEx, "Cache was disposed unexpectedly when getting a value");
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// Tries to get a value from the IMemoryCache
        /// </summary>
        /// <param name="key">The key that identifies the value in the cache</param>
        /// <param name="value">The value to save in the cache</param>
        /// <param name="value">Memory cache options to set expiration. If not set, expiration for the lifetime set in appsettings.json is used.</param>
        /// <returns>The value</returns>
        public T Set<T>(string key, T value, MemoryCacheEntryOptions options = null)
        {
            options = options ?? new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheLifetime));

            try
            {
                return _cache.Set(key, value, options);
            }
            catch (ObjectDisposedException odEx)
            {
                _logger.LogWarning(odEx, "Cache was disposed unexpectedly when setting a value");
                return default(T);
            }
        }
    }
}
