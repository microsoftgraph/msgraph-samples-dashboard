// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.KeyVaultExtensions;
using Microsoft.IdentityModel.Tokens;
using Octokit;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// Handles GitHub authentication functionality
    /// </summary>
    public class GitHubAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GitHubAuthService> _logger;

        public GitHubAuthService(
          IConfiguration configuration,
          IMemoryCache memoryCache,
          ILogger<GitHubAuthService> logger)
        {
            _configuration = configuration;
            _cache = memoryCache;
            _logger = logger;
        }

        public async Task<GitHubClient> GetAuthenticatedClient()
        {
            var token = await GetGitHubAppToken();

            return new GitHubClient(
                new ProductHeaderValue(_configuration.GetValue<string>(Constants.Product),
                                       _configuration.GetValue<string>(Constants.ProductVersion)))
            {
                Credentials = new Credentials(token)
            };
        }

        /// <summary>
        /// Generates an installation token for the GitHub app installation instance
        /// </summary>
        /// <returns>installation token</returns>
        internal async Task<string> GetGitHubAppToken()
        {
            if (_cache.TryGetValue(Constants.GitHubToken, out string gitHubToken))
            {
                return gitHubToken;
            }

            try
            {
                // This is token to get a token :D
                var jwtToken = await GetSignedJwtAsync();

                var gitHubClient = new GitHubClient(
                    new ProductHeaderValue(_configuration.GetValue<string>(Constants.Product),
                                           _configuration.GetValue<string>(Constants.ProductVersion)))
                {
                    Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
                };

                // Get the installation ID of the app in the org
                var installations = await gitHubClient.GitHubApps.GetAllInstallationsForCurrent();
                var installId = installations
                    .Where(i => i.Account.Login == _configuration.GetValue<string>(Constants.GitHubOrg))
                    .FirstOrDefault().Id;

                // Create an installation token (GitHub equivalent of app-only)
                var tokenResponse = await gitHubClient.GitHubApps.CreateInstallationToken(installId);
                gitHubToken = tokenResponse.Token;

                // Cache the token for as long as it's valid
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(tokenResponse.ExpiresAt);
                _cache.Set(Constants.GitHubToken, gitHubToken, cacheEntryOptions);

                return gitHubToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not get GitHub app token");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets key, generates and signs a JWT that will allow an API
        /// call to generate an installation token
        /// </summary>
        /// <returns>signed JWT</returns>
        private async Task<string> GetSignedJwtAsync()
        {
            SigningCredentials signingCredentials = null;

            // Get the GitHub app's private key
            var pemFile = _configuration.GetValue<string>(Constants.GitHubAppKeyFile);
            if (!string.IsNullOrEmpty(pemFile))
            {
                // Load from file
                // Should only be used for local development/testing
                _logger.LogDebug($"Loading private key locally from {pemFile}");
                var privateKey = await File.ReadAllTextAsync(pemFile);
                var rsa = RSA.Create();
                rsa.ImportFromPem(privateKey.ToCharArray());
                signingCredentials = new SigningCredentials(
                    new RsaSecurityKey(rsa),
                    SecurityAlgorithms.RsaSha256);
            }
            else
            {
                // Load key from Azure Key Vault
                var credential = new ClientSecretCredential(
                    _configuration.GetValue<string>(Constants.AzureTenantId),
                    _configuration.GetValue<string>(Constants.KeyVaultAppId),
                    _configuration.GetValue<string>(Constants.KeyVaultSecret)
                );

                var keyVaultClient = new KeyClient(
                    new Uri(_configuration.GetValue<string>(Constants.KeyVaultUri)),
                    credential);

                var key = await keyVaultClient
                    .GetKeyAsync(_configuration.GetValue<string>(Constants.GitHubAppKeyIdentifier));

                // Key returned from Azure is public key only
                // To do signing, you need to let Azure Key Vault do the signing
                // for you. This avoids ever having the private key in our code
                signingCredentials = new SigningCredentials(
                    new KeyVaultSecurityKey(key.Value.Id.ToString(), async (_,_,_) => {
                        var context = new TokenRequestContext(Constants.KeyVaultScopes);
                        var token = await credential.GetTokenAsync(context);
                        return token.Token;
                    }), SecurityAlgorithms.RsaSha256)
                    {
                        CryptoProviderFactory = new CryptoProviderFactory
                        {
                            CustomCryptoProvider = new KeyVaultCryptoProvider()
                        }
                    };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddSeconds(600),
                Issuer = _configuration.GetValue<string>(Constants.GitHubAppId),
                SigningCredentials= signingCredentials
            };

            // This is token to get a token :D
            var jwtToken = tokenHandler.CreateEncodedJwt(tokenDescriptor);
            return jwtToken;
        }
    }
}