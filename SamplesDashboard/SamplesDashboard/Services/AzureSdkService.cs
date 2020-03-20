// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        private IMemoryCache _cache;
        private readonly IConfiguration _config;

        public AzureSdkService(IHttpClientFactory clientFactory, IMemoryCache cache)
        {
            _clientFactory = clientFactory;
            _cache = cache;
        } 

        /// <summary>
        /// Downloads and parses an xml file to get a dictionary of azure sdk versions
        /// </summary>
        /// <returns> a dictionary of packages</returns>
        public async Task<Dictionary<string, string>> FetchAzureSdkVersions()
        {
            Dictionary<string, string> packages = new Dictionary<string, string>();

            if (!_cache.TryGetValue("azureSdkVersions", out packages))
            {
                var httpClient = _clientFactory.CreateClient();

                HttpResponseMessage responseMessage = await httpClient.GetAsync("https://raw.githubusercontent.com/Azure/azure-sdk-for-net/master/eng/Packages.Data.props");

                if (!responseMessage.IsSuccessStatusCode)
                {
                    return null;
                }
                string fileContents = await responseMessage.Content.ReadAsStringAsync();
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(fileContents);

                XmlNodeList data = xml.GetElementsByTagName("PackageReference");

                foreach (XmlNode node in data)
                {
                    if (!packages.ContainsKey(node.Attributes["Update"].Value))
                    {
                        packages.Add(node.Attributes["Update"].Value, node.Attributes["Version"].Value);
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
