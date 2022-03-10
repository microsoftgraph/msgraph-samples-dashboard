// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class contains cocoapods.org queries to be used by the samples
    /// </summary>
    public class CocoaPodsService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly CacheService _cacheService;

        public CocoaPodsService(
            IHttpClientFactory clientFactory,
            CacheService cacheService)
        {
            _clientFactory = clientFactory;
            _cacheService = cacheService;
        }

        /// <summary>
        ///Creates a httpclient to query cocoapods.org to get package details
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns>latest package version</returns>
        public async Task<string> GetLatestVersion(string packageName)
        {
            var cacheKey = $"cocoapods:{packageName}";
            if (!_cacheService.TryGetValue(cacheKey, out string packageVersion))
            {
                var httpClient = _clientFactory.CreateClient("Default");

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
                                _cacheService.Set(cacheKey, packageVersion);
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
