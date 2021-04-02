// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using SamplesDashboard.Models;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class contains query services for the Microsoft Open Source GitHub portal
    /// </summary>
    public class MicrosoftOpenSourceService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<MicrosoftOpenSourceService> _logger;
        private readonly IConfidentialClientApplication _cca;
        private static readonly string[] _scopes =
            { "api://5bc5e692-fe67-4053-8d49-9e2863718bfb/.default" };

        public MicrosoftOpenSourceService(
          IHttpClientFactory clientFactory,
          IConfiguration configuration,
          ILogger<MicrosoftOpenSourceService> logger)
        {
            _clientFactory = clientFactory;
            _config = configuration;
            _logger = logger;

            _cca = ConfidentialClientApplicationBuilder
                .Create(_config["AuthenticationConfig:Microsoft:ClientId"])
                .WithClientSecret(_config["AuthenticationConfig:Microsoft:ClientSecret"])
                .WithTenantId(_config["AuthenticationConfig:Microsoft:TenantId"])
                .Build();
        }

        /// <summary>
        /// Gets the maintainer status for a given repository
        /// <summary>
        /// <param name="organization"></param>
        /// <param name="repoName"></param>
        /// <returns> The individuals and/or security group configured as maintainers in the portal
        public async Task<MicrosoftMaintainerStatus> GetMicrosoftMaintainers(string organization, string repoName)
        {
            try
            {
                var tokenResponse = await _cca
                .AcquireTokenForClient(_scopes)
                .ExecuteAsync();

                var client = _clientFactory.CreateClient();

                var apiPath =
                    $"https://repos.opensource.microsoft.com/api/maintainers/org/{organization}/repo/{repoName}?api-version=2017-09-01";
                var request = new HttpRequestMessage(HttpMethod.Get, apiPath);
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

                var apiResponse = await client.SendAsync(request);

                if (apiResponse.IsSuccessStatusCode)
                {
                    var result = JsonConvert
                        .DeserializeObject<MicrosoftMaintainerStatus>(
                            await apiResponse.Content.ReadAsStringAsync());

                    return result;
                }
            }
            catch (MsalException ex)
            {
                _logger.LogError(ex,
                    $"Error getting Microsoft maintainers for {repoName}");
            }

            return null;
        }
    }
}
