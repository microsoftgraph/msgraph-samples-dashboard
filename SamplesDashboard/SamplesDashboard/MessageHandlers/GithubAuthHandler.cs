// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using SamplesDashboard.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SamplesDashboard.MessageHandlers
{
    public class GithubAuthHandler : DelegatingHandler
    {
        private readonly GithubAuthService _githubAuthService;

        public GithubAuthHandler(GithubAuthService githubAuthService)
        {
            _githubAuthService = githubAuthService;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _githubAuthService.GetGithubAppToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }
        
    }
}
