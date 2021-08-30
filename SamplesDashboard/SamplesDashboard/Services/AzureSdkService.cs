// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class contains methods for handling azure sdk versions
    /// </summary>
    public class AzureSdkService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public AzureSdkService(IHttpClientFactory clientFactory, IMemoryCache cache, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _cache = cache;
            _config = config;
        } 

        /// <summary>
        /// Downloads and parses an xml file to get a dictionary of azure sdk versions
        /// </summary>
        /// <returns> a dictionary of packages</returns>
        internal async Task<Dictionary<string, string>> FetchAzureSdkVersions()
        {
            Dictionary<string, string> packages;

            if (!_cache.TryGetValue("azureSdkVersions", out packages))
            {
                packages = new Dictionary<string, string>();
                var httpClient = _clientFactory.CreateClient();

                HttpResponseMessage responseMessage = await httpClient.GetAsync("https://raw.githubusercontent.com/Azure/azure-sdk-for-net/master/eng/Packages.Data.props");

                if (!responseMessage.IsSuccessStatusCode)
                {
                    return null;
                }

                XmlDocument xml = new XmlDocument();
                Stream fileStream = await responseMessage.Content.ReadAsStreamAsync();
                xml.Load(fileStream);

                XmlNodeList data = xml.GetElementsByTagName("PackageReference");
                foreach (XmlNode node in data)
                {
                    var attribute = node.Attributes["Update"];
                    if (attribute == null) continue;
                    if (node.Attributes["Version"] == null) continue;

                    if (!packages.ContainsKey(attribute.Value))
                    {
                        packages.Add(attribute.Value, node.Attributes["Version"].Value);
                    }
                }
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));
                _cache.Set("azureSdkVersions", packages, cacheEntryOptions);
            }

            return packages;
        }

        /// <summary>
        /// This method gets a dictionary of packages and sets the version used
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns>azure sdk version</returns>
        public async Task<string> GetAzureSdkVersions(string packageName)
        {
            string azureSdkVersion = String.Empty;

            var sdks = await FetchAzureSdkVersions();
            if (sdks == null)
            {
                return  String.Empty;  
            }
            foreach (var sdk in sdks)
            {
                if (sdk.Key == packageName)
                {
                    azureSdkVersion = sdk.Value;
                }
            }
            return azureSdkVersion;
        }
    }
}
