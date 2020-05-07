// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Octokit;
using SamplesDashboard.Helper;
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
        private static readonly long TicksSince197011 = new DateTime(1970, 1, 1).Ticks;

        public GithubAuthService(IMemoryCache memoryCache,IConfiguration configuration)
        {
            _cache = memoryCache;
            _configuration = configuration;
        }

        private KeyVaultClient GetAzureKeyVaultClient() => new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(async (string authority, string resource, string scope) =>
        {

            var CLIENT_ID = _configuration["AuthenticationConfig:AzureKeyVault:ClientId"];
            var CLIENT_SECRET = _configuration["AuthenticationConfig:AzureKeyVault:ClientSecret"];

            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            ClientCredential clientCred = new ClientCredential(CLIENT_ID, CLIENT_SECRET);
            var authResult = await context.AcquireTokenAsync(resource, clientCred);
            return authResult.AccessToken;
        }));

        internal async Task<string> GetGithubAppToken()
        {
            string token;
            if (!_cache.TryGetValue("githubToken", out token))
            {
                var KeyIdentifier = _configuration["KeyIdentifier"];

                //create azurekeyvault client
                var client = GetAzureKeyVaultClient();
                var certificateBundle = await client.GetSecretAsync(KeyIdentifier);

                //insert missing newlines that cause a problem on reading the certificate
                var sections = certificateBundle.Value.Split("-----BEGIN RSA PRIVATE KEY-----", StringSplitOptions.RemoveEmptyEntries);
                sections = sections[0].Split("-----END RSA PRIVATE KEY-----", StringSplitOptions.RemoveEmptyEntries);

                //insert missing newlines that cause a problem on reading the certificate
                string key = "-----BEGIN RSA PRIVATE KEY-----\r\n" + sections[0] + "\r\n-----END RSA PRIVATE KEY-----";

                var utcNow = DateTime.UtcNow;
                var payload = new Dictionary<string, object>
                {
                    {"iat", ToUtcSeconds(utcNow)},
                    {"exp", ToUtcSeconds(utcNow.AddSeconds(600))},
                    {"iss", 62050}
                };

                var jwtToken = JWTHelper.CreateEncodedJwtToken(key, payload);

                // Pass the JWT as a Bearer token to Octokit.net
                var appClient = new GitHubClient(new ProductHeaderValue(_configuration.GetValue<string>("product")))
                {
                    Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
                };

                // Get a list of installations for the authenticated GitHubApp and installationID for microsoftgraph
                var installations = await appClient.GitHubApps.GetAllInstallationsForCurrent();
                var id = installations.Where(installation => installation.Account.Login == "microsoftgraph").FirstOrDefault().Id;

                // Create an Installation token for the microsoftgraph installation instance
                var response = await appClient.GitHubApps.CreateInstallationToken(id);
                token = response.Token;

                //set cache to expire at the same time as the token
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(response.ExpiresAt);
                _cache.Set("githubToken", token, cacheEntryOptions);
            }

            return token;
        }
        private static long ToUtcSeconds(DateTime dt)
        {
            return (dt.ToUniversalTime().Ticks - TicksSince197011) / TimeSpan.TicksPerSecond;
        }
    }
}
