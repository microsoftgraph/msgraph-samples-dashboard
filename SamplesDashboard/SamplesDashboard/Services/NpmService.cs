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
    /// This class contains NPM queries to be used by the samples
    /// </summary>
    public class NpmService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private IMemoryCache _cache;

        public NpmService(IHttpClientFactory clientFactory, IConfiguration config, IMemoryCache memoryCache)
        {
            _clientFactory = clientFactory;
            _config = config;
            _cache = memoryCache;
        }

        /// <summary>
        ///Creates a httpclient to query npm's registry to get package details
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns>latest package version</returns>
        public async Task<string> GetLatestVersion(string packageName)
        {
            var httpClient = _clientFactory.CreateClient();
         
            HttpResponseMessage responseMessage = await httpClient.GetAsync(string.Concat("https://registry.npmjs.org/", packageName));

            if (responseMessage.IsSuccessStatusCode)
            {
                NpmQuery npmData;
                if (!_cache.TryGetValue(responseMessage, out npmData))
                {
                    using (var stream = await responseMessage.Content.ReadAsStreamAsync())
                    using (var streamReader = new StreamReader(stream))
                    using (var jsonTextReader = new JsonTextReader(streamReader))

                    {
                        var serializer = new JsonSerializer();
                        npmData = serializer.Deserialize<NpmQuery>(jsonTextReader);

                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>("timeout")));
                        _cache.Set(responseMessage, npmData, cacheEntryOptions);
                    }                   
                }

                return npmData.DistTags.Latest;
            }

            return null;
        }
    }
}
