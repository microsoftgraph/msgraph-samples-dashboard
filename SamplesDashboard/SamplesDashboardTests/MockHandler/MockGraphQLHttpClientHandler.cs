// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SampleDashboardTests.MockHandler
{
    public class MockGraphQLHttpClientHandler : DelegatingHandler
    {
        public string SamplesResponse { get; set; } = string.Empty;
        public string SdksResponse { get; set; } = string.Empty;

        public string GetRepoResponse { get; set; } = string.Empty;

        public string RequestUri { get; set; } = string.Empty;

        private const string repoNamePattern = @"""repo""\s*:\s*""([^""]*)""";

        public MockGraphQLHttpClientHandler(string requestUri, string samplesResponse, string sdksResponse, string getRepoResponse)
        {
            RequestUri = requestUri;
            SamplesResponse = samplesResponse;
            SdksResponse = sdksResponse;
            GetRepoResponse = getRepoResponse;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri.AbsoluteUri == RequestUri)
            {
                string responseJson = string.Empty;

                var requestBody = await request.Content.ReadAsStringAsync();

                if (requestBody.ToLower().Contains("stargazer"))
                {
                    responseJson = GetRepoResponse;

                    var match = Regex.Match(requestBody, repoNamePattern, RegexOptions.Multiline);
                    if (match != null && match.Success)
                    {
                        var requestedRepo = match.Groups[1].Value;
                        responseJson = responseJson.Replace("msgraph-training-aspnet-core", requestedRepo, true,
                            System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                else if (requestBody.ToLower().Contains("training"))
                {
                    responseJson = SamplesResponse;
                }
                else if (requestBody.ToLower().Contains("sdk"))
                {
                    responseJson = SdksResponse;
                }

                return new HttpResponseMessage
                {
                    Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
        }
    }
}
