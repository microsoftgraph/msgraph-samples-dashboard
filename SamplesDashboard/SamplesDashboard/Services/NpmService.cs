// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using SamplesDashboard.Models;

namespace SamplesDashboard.Services
{
    public class NpmService
    {
        private const string npmRegistry = "https://registry.npmjs.org/";
        private readonly IHttpClientFactory _clientFactory;
        private readonly CacheService _cacheService;
        private readonly JsonSerializerOptions _jsonOptions;

        public NpmService(
            IHttpClientFactory clientFactory,
            CacheService cacheService)
        {
            _clientFactory = clientFactory;
            _cacheService = cacheService;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        public async Task<string> GetLatestVersion(string packageName)
        {
            var cacheKey = $"npm:{packageName}";
            if (!_cacheService.TryGetValue(cacheKey, out string latestVersion))
            {
                var httpClient = _clientFactory.CreateClient("Default");

                var response = await httpClient.GetAsync($"{npmRegistry}{packageName}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStreamAsync();
                    var queryResult = await JsonSerializer.DeserializeAsync<NpmQueryResult>(jsonResponse, _jsonOptions);

                    latestVersion = queryResult?.Tags?.Latest ?? string.Empty;
                    // Save the versions into the cache
                    _cacheService.Set(cacheKey, latestVersion);
                }
            }

            return latestVersion;
        }
    }
}
