// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace SamplesDashboard.Policies
{
    public static class GitHubRetryPolicy
    {
        private static IAsyncPolicy<HttpResponseMessage> _retryPolicy;

        private static IAsyncPolicy<HttpResponseMessage> GetPolicy()
        {
            return HttpPolicyExtensions
              .HandleTransientHttpError()
              .OrResult(msg => msg.StatusCode == HttpStatusCode.Forbidden)
              .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        /// <summary>
        /// An HTTP retry policy to retry HTTP requests when we encounter HTTP 403 and 503,
        /// with exponential back-off
        /// Useful due to transient networking issues on the cloud.
        /// </summary>
        /// <returns>The policy</returns>
        public static IAsyncPolicy<HttpResponseMessage> Policy => _retryPolicy ??= GetPolicy();
    }
}
