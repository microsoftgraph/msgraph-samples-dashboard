// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SamplesDashboard.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class contains search.maven.org queries to be used by the samples
    /// </summary>
    public class MavenService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public MavenService(IHttpClientFactory clientFactory, IConfiguration config, IMemoryCache memoryCache)
        {
            _clientFactory = clientFactory;
            _config = config;
            _cache = memoryCache;
        }

        /// <summary>
        ///Creates a httpclient to query Maven's registry to get package details
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns>latest package version</returns>
        public async Task<string> GetLatestVersion(string packageName)
        {
            if (!_cache.TryGetValue($"maven: {packageName}", out MavenQuery mavenData))
            {
                var httpClient = _clientFactory.CreateClient();

                var packageParts = packageName.Split(':');
                var apiUrl = $"https://search.maven.org/solrsearch/select?q=g:%22{packageParts[0]}%22%20AND%20a:%22{packageParts[1]}%22&rows=1&wt=json";
                var responseMessage = await httpClient.GetAsync(apiUrl);

                if (responseMessage.IsSuccessStatusCode)
                {

                    await using var stream = await responseMessage.Content.ReadAsStreamAsync();
                    using (var streamReader = new StreamReader(stream))
                    using (var jsonTextReader = new JsonTextReader(streamReader))

                    {
                        var serializer = new JsonSerializer();
                        mavenData = serializer.Deserialize<MavenQuery>(jsonTextReader);

                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));
                        _cache.Set($"maven: {packageName}", mavenData, cacheEntryOptions);
                    }
                }
            }

            return mavenData?.Response?.NumFound > 0?
                mavenData?.Response?.Docs[0]?.LatestVersion : "";
        }
    }
}
