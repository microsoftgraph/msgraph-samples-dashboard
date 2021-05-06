// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SamplesDashboard.Models;

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

                // Check Google's Maven repo first for any Android packages
                mavenData = await GetLatestAndroidPackageVersion(httpClient, packageParts[0], packageParts[1]);
                if (mavenData == null)
                {
                    // Adding &core=gav will return all versions, but doesn't return latestVersion :|
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
                        }
                    }
                }

                if (mavenData != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));
                    _cache.Set($"maven: {packageName}", mavenData, cacheEntryOptions);
                }
            }

            return mavenData?.Response?.NumFound > 0?
                mavenData?.Response?.Docs[0]?.LatestVersion : "";
        }

        private async Task<MavenQuery> GetLatestAndroidPackageVersion(HttpClient client, string groupName, string packageName)
        {
            var groupParts = groupName.Split('.');

            var googleUrl = $"https://dl.google.com/android/maven2/{string.Join('/', groupParts)}/group-index.xml";

            var responseMessage = await client.GetAsync(googleUrl);

            if (responseMessage.IsSuccessStatusCode)
            {

                await using var stream = await responseMessage.Content.ReadAsStreamAsync();
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(stream);

                var packageNodes = xmlDocument.GetElementsByTagName(packageName);
                for (int i = 0; i < packageNodes.Count; i++)
                {
                    var node = packageNodes[i];
                    var versionNode = node.Attributes.GetNamedItem("versions");

                    var versions = versionNode.InnerText.Split(',');
                    var latestVersion = versions?.LastOrDefault();

                    if (!string.IsNullOrEmpty(latestVersion))
                    {
                        return new MavenQuery
                        {
                            Response = new MavenResponse
                            {
                                NumFound = 1,
                                Docs = new List<MavenPackageInfo>
                                {
                                    new MavenPackageInfo 
                                    {
                                        Id = $"{groupName}:{packageName}",
                                        LatestVersion = latestVersion
                                    }
                                }
                            }
                        };
                    }
                }
            }

            return null;
        }
    }
}
