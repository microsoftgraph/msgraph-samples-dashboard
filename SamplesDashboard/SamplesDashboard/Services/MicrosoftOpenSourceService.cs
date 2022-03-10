// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
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
        private readonly JsonSerializerOptions _jsonOptions;

        // This single scope is the permission scope needed to call the
        // Microsoft Open Source GitHub portal API. Don't change this.
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
                .Create(_config.GetValue<string>(Constants.MSOSClientId))
                .WithClientSecret(_config.GetValue<string>(Constants.MSOSClientSecret))
                .WithTenantId(_config.GetValue<string>(Constants.AzureTenantId))
                .Build();
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
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

                if (tokenResponse == null)
                {
                    throw new MsalException(MsalError.AuthenticationFailed, "AcquireTokenForClient returned null");
                }

                var client = _clientFactory.CreateClient("Default");

                var apiPath =
                    $"https://repos.opensource.microsoft.com/api/maintainers/org/{organization}/repo/{repoName}?api-version=2017-09-01";
                var request = new HttpRequestMessage(HttpMethod.Get, apiPath);
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

                var apiResponse = await client.SendAsync(request);

                if (apiResponse.IsSuccessStatusCode)
                {
                    var result = await JsonSerializer
                        .DeserializeAsync<MicrosoftMaintainerStatus>(
                            await apiResponse.Content.ReadAsStreamAsync(),
                            _jsonOptions
                        );

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