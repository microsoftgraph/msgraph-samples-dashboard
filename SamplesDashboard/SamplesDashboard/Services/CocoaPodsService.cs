// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class contains cocoapods.org queries to be used by the samples
    /// </summary>
    public class CocoaPodsService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public CocoaPodsService(IHttpClientFactory clientFactory, IConfiguration config, IMemoryCache memoryCache)
        {
            _clientFactory = clientFactory;
            _config = config;
            _cache = memoryCache;
        }

        /// <summary>
        ///Creates a httpclient to query cocoapods.org to get package details
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns>latest package version</returns>
        public async Task<string> GetLatestVersion(string packageName)
        {
            if (!_cache.TryGetValue($"cocoapods: {packageName}", out string packageVersion))
            {
                var httpClient = _clientFactory.CreateClient();

                var apiUrl = $"https://cocoapods.org/pods/{packageName}";
                var responseMessage = await httpClient.GetAsync(apiUrl);

                if (responseMessage.IsSuccessStatusCode)
                {
                    await using var stream = await responseMessage.Content.ReadAsStreamAsync();
                    using (var streamReader = new StreamReader(stream))
                    {
                        var html = await streamReader.ReadToEndAsync();

                        // Load the HTML into an AngleSharp context for parsing
                        var browsingContext = BrowsingContext.New(Configuration.Default);
                        var htmlDoc = await browsingContext.OpenAsync(req => req.Content(html));

                        // Version is inside a <span> inside an <h1>
                        var h1Elements = htmlDoc.All.Where(e => e.LocalName == "h1"  && e.ChildElementCount == 1);
                        foreach (var h1 in h1Elements)
                        {
                            if (h1.FirstElementChild.LocalName == "span")
                            {
                                // Found the version
                                packageVersion = h1.FirstElementChild.InnerHtml;
                                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                                    TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));
                                _cache.Set($"cocoapods: {packageName}", packageVersion, cacheEntryOptions);
                                return packageVersion;
                            }
                        }
                    }
                }
            }

            return packageVersion ?? "";
        }
    }
}
