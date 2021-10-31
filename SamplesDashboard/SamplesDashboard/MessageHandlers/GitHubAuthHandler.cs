// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using SamplesDashboard.Services;

namespace SamplesDashboard.MessageHandlers
{
    /// <summary>
    /// Adds authentication to outgoing requests for GraphQL clients
    /// </summary>
    public class GitHubAuthHandler : DelegatingHandler
    {
        private readonly GitHubAuthService _gitHubAuthService;
        public GitHubAuthHandler(GitHubAuthService gitHubAuthService)
        {
            _gitHubAuthService = gitHubAuthService;
        }

        /// <summary>
        /// Adds a GitHub app-only token in the Authorization header of
        /// outgoing requests
        /// </summary>
        /// <param name="requestMessage">The HTTP request to authenticate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The HTTP response</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage requestMessage,
          CancellationToken cancellationToken
        )
        {
            var token = await _gitHubAuthService.GetGitHubAppToken();
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            return await base.SendAsync(requestMessage, cancellationToken);
        }
    }
}
