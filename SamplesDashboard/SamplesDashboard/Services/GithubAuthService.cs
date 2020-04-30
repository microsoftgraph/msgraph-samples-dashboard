using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SamplesDashboard.Services
{
    public class GithubAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public GithubAuthService(IMemoryCache memoryCache,IConfiguration configuration)
        {
            _cache = memoryCache;
            _configuration = configuration;
        }

        internal async Task<string> GetGithubAppToken()
        {
            string token;
            if (!_cache.TryGetValue("githubToken", out token))
            {
                // Use GitHubJwt library to create the GitHubApp Jwt Token using our private certificate PEM file
                var generator = new GitHubJwt.GitHubJwtFactory(
                    new GitHubJwt.FilePrivateKeySource(@"C:\Users\v-makim\Downloads\devx-dashboard.2020-04-30.private-key.pem"),
                    new GitHubJwt.GitHubJwtFactoryOptions
                    {
                        AppIntegrationId = 62050, // The GitHub App Id
                        ExpirationSeconds = 600 // 10 minutes is the maximum time allowed
                    });

                var jwtToken = generator.CreateEncodedJwtToken();

                // Pass the JWT as a Bearer token to Octokit.net
                var appClient = new GitHubClient(new Octokit.ProductHeaderValue(_configuration.GetValue<string>("product")))
                {
                    Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
                };

                // Get a list of installations for the authenticated GitHubApp and installationID for microsoftgraph
                var installations = await appClient.GitHubApps.GetAllInstallationsForCurrent();
                var id = installations.Where(installation => installation.Account.Login == "microsoftgraph").FirstOrDefault().Id;

                // Create an Installation token for the microsoftgraph installation instance
                var response = await appClient.GitHubApps.CreateInstallationToken(id);
                token = response.Token;
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(response.ExpiresAt);
                _cache.Set("githubToken", token, cacheEntryOptions);
            }
            return token;
        }
    }
}
