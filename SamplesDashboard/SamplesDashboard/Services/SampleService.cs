// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System;
using GraphQL;
using GraphQL.Client.Http;

namespace SamplesDashboard.Services
{
    /// <summary>
    ///  This class contains samples query services and functions to be used by the samples API
    /// </summary>
    public class SampleService
    {
        private readonly GraphQLHttpClient _client;

        public SampleService(GraphQLHttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Gets the client object used to run the samples query and return the samples list 
        /// </summary>
        /// <param name="client">The GraphQLHttpClient</param>
        /// <returns> A list of samples.</returns>
        public async Task<List<Node>> GetSamples()
        { 
            // Request to fetch the list of samples for graph
            var request = new GraphQLRequest
            {
                Query = @"
	            {
                  search(query: ""org:microsoftgraph sample OR training in:name archived:false"", type: REPOSITORY, first: 100) {
                        nodes {
                                ... on Repository {
                                    id
                                    name
                                    owner {
                                        login
                                    }
                                    vulnerabilityAlerts {
                                        totalCount
                                    }
                                    issues(states: OPEN) {
                                        totalCount
                                    }
                                    pullRequests(states: OPEN) {
                                        totalCount
                                    }
                                    stargazers {
                                        totalCount
                                    }
                                }
                            }
                        }
                }"
            };
            var graphQLResponse = await _client.SendQueryAsync<Data>(request);

            // Fetch yaml headers and compute header values in parallel
            List<Task> TaskList = new List<Task>();
            foreach (var sampleItem in graphQLResponse?.Data?.Search.Nodes)
            {
                Task headerTask = SetHeaders(sampleItem);
                TaskList.Add(headerTask);
            }

            await Task.WhenAll(TaskList);
            return graphQLResponse?.Data?.Search.Nodes;
        }

        /// <summary>
        ///Getting header details and setting the language and featureArea items        
        /// </summary>
        /// <param name="sampleItem">A specific sample item from the samples list</param>
        /// <returns> A list of samples.</returns>
        private async Task SetHeaders(Node sampleItem) 
        {
            var headerDetails = await GetHeaderDetails(sampleItem.Name);
            sampleItem.Language = headerDetails.GetValueOrDefault("languages");
            sampleItem.FeatureArea = headerDetails.GetValueOrDefault("services");
        }

        /// <summary>
        /// Uses client object and sampleName passed inthe url to return the sample's dependencies
        /// </summary>
        /// <param name="client">The GraphQLHttpClient</param>
        /// <param name="sampleName">The name of that sample</param>
        /// <returns> A list of dependencies. </returns>
        public async Task<IEnumerable<DependenciesNode>> GetDependencies(string sampleName)
        {
            //request to fetch sample dependencies
            var request = new GraphQLRequest
            {
                Query = @"query Sample($sample: String!){
                        organization(login: ""microsoftgraph"") {
                            repository(name: $sample){
                                dependencyGraphManifests(withDependencies: true) {
                                    nodes {
                                        filename
                                        dependencies(first: 10) {
                                            nodes {
                                                packageManager
                                                packageName
                                                requirements
                                                repository{
                                                  name
                                                  releases(last: 1){
                                                      nodes{
                                                          name
                                                          tagName
                                                      }
                                                  }                 
                                                }
                                            }
                                        }
                                    }
                                }
                            }                        
                    }
                }",
                Variables = new { sample = sampleName }
            };

            var graphQLResponse = await _client.SendQueryAsync<Data>(request);

            if (graphQLResponse.Data.Organization.Repository != null)
            {
                var dependencies =
                    graphQLResponse.Data.Organization.Repository.DependencyGraphManifests.Nodes.SelectMany(n =>
                        n.Dependencies.Nodes);
                return dependencies;
            }

            return null;
        }       

        /// <summary>
        /// Get header details list from the parsed yaml header
        /// </summary>
        /// <param name="sampleName">The name of each sample</param>
        /// <returns> A new list of services in the yaml header after parsing it.</returns>
        public async Task<Dictionary<string,string>> GetHeaderDetails(string sampleName)
        {
            string header = await GetYamlHeader(sampleName);
            if (!string.IsNullOrEmpty(header))
            {
                string[] lines = header.Split("\r\n");
                string[] details = new string[] { "languages", "services" };

                Dictionary<string, string> keyValuePairs = new Dictionary<string,string>();
                for (int i = 0; i < details.Length; i++)
                {
                    var res = SearchTerm(details[i], lines);
                    var stringList = string.Join(',', res);
                    keyValuePairs.Add(details[i], stringList);
                }
                return keyValuePairs;
            }
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// fetch yaml header from sample repo and parse it
        /// </summary>
        /// <param name="sampleName">The name of each sample</param>
        /// <returns> The yaml header. </returns>
        private async Task<string> GetYamlHeader(string sampleName)
        {
            HttpClient httpClient = new HttpClient();

            //downloading the yaml file
            HttpResponseMessage responseMessage = await httpClient.GetAsync(string.Concat("https://raw.githubusercontent.com/microsoftgraph/", sampleName, "/master/Readme.md"));

            if (responseMessage.StatusCode.ToString().Equals("NotFound"))
                responseMessage = await httpClient.GetAsync(string.Concat("https://raw.githubusercontent.com/microsoftgraph/", sampleName, "/master/README.md"));

            if (responseMessage.IsSuccessStatusCode)
            {
                string fileContents = await responseMessage.Content.ReadAsStringAsync();
                string[] parts = fileContents.Split("---", StringSplitOptions.RemoveEmptyEntries);
                //we have a valid header between ---
                if (parts.Length > 1)
                {
                    return parts[0];
                }
            }
            return "";
        }

        /// <summary>
        /// Gets the searchterm and gets related entries.
        /// </summary>
        /// <param name="term">The header details we're parsing</param>
        /// <param name="lines">The header lines after splitting</param>
        /// <returns>A list of the searchterm specified.</returns>
        private List<string> SearchTerm(string term, string[] lines)
        {
            bool foundHeader = false;
            List<string> myList = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains(term))
                {
                    foundHeader = true;
                    continue;
                }
                if (foundHeader)
                {
                    if (line.Contains(":"))
                        break;
                    myList.Add(line.Split("-", StringSplitOptions.RemoveEmptyEntries).Last().Trim());
                }
            }
            return myList;
        }
    }
}
