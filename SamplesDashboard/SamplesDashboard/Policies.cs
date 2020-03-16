using System;
using System.Net;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace SamplesDashboard
{
    public class Policies
    {
        private static IAsyncPolicy<HttpResponseMessage> _githubRetryPolicy;
        public static IAsyncPolicy<HttpResponseMessage> GithubRetryPolicy => _githubRetryPolicy ??= GetGithubRetryPolicy();
        /// <summary>
        /// A Http Retry policy to retry Http Requests when we encounter HTTP 404 and 503, with exponential back-off
        /// Useful due to transient networking issues on the cloud. 
        /// </summary>
        /// <returns></returns>
        private static IAsyncPolicy<HttpResponseMessage> GetGithubRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg =>
                    msg.StatusCode == HttpStatusCode.Forbidden)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)));
        }
    }
}
