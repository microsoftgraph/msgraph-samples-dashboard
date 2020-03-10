// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Newtonsoft.Json;
using SamplesDashboard.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class contains queries for NPM  to be used by the samples
    /// </summary>
    public class NpmService
    {
        private readonly IHttpClientFactory _clientFactory;

        public NpmService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
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
                string fileContents = await responseMessage.Content.ReadAsStringAsync();
                var npmData = JsonConvert.DeserializeObject<NpmQuery>(fileContents);
                return npmData.DistTags.Latest;
            }

            return null;
        }
    }
}
