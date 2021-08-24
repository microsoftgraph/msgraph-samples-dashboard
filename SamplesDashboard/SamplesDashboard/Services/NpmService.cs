// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SamplesDashboard.Models;

namespace SamplesDashboard.Services
{
    public class NpmService
    {
        private const string npmRegistry = "https://registry.npmjs.org/";
        private readonly IHttpClientFactory _clientFactory;
        private readonly double _cacheLifetime;
        private readonly IMemoryCache _cache;
        private readonly JsonSerializerOptions _jsonOptions;

        public NpmService(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            IMemoryCache memoryCache)
        {
            _clientFactory = clientFactory;
            _cacheLifetime = configuration.GetValue<double>(Constants.CacheLifetime);
            _cache = memoryCache;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        public async Task<string> GetLatestVersion(string packageName)
        {
            var cacheKey = $"npm:{packageName}";
            if (!_cache.TryGetValue(cacheKey, out string latestVersion))
            {
                var httpClient = _clientFactory.CreateClient();

                var response = await httpClient.GetAsync($"{npmRegistry}{packageName}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStreamAsync();
                    var queryResult = await JsonSerializer.DeserializeAsync<NpmQueryResult>(jsonResponse, _jsonOptions);

                    latestVersion = queryResult?.Tags?.Latest ?? string.Empty;
                    // Save the versions into the cache
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheLifetime));
                    _cache.Set(cacheKey, latestVersion);
                }
            }

            return latestVersion;
        }
    }
}
