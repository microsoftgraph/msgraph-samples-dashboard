// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SamplesDashboardTests.MessageHandlers
{
    /// <summary>
    /// An HttpMessageHandler that returns strings from an array in sequence.
    /// Once the end of the array is reached, subsequent requests return 404
    /// </summary>
    public class StringContentHandler : DelegatingHandler
    {
        private int _index = 0;
        private string[] _content;

        public StringContentHandler(string[] content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                               CancellationToken cancellationToken)
        {

            if (_index < _content.Length)
            {
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    // Return string at current index and increment index
                    // for the next request
                    Content = new StringContent(_content[_index++])
                });
            }

            // Once the end of array has been reached, return 404
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.NotFound
            });
        }
    }
}
