// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SamplesDashboard.Models;
using System.Net.Http;
using System;
using GraphQL.Client;
using GraphQL.Common.Request;
using Newtonsoft.Json;

namespace SamplesDashboard.Services
{
    /// <summary>
    ///  This class contains samples query services and functions to be used by the samples API
    /// </summary>
    public static class SampleService
    {

        /// <summary>
        /// Gets the client object used to run the samples query and return the samples list 
        /// </summary>
        /// <param name="client"></param>
        /// <returns> A list of samples.</returns>
        public static async Task<List<Repo>> GetSamples(GraphQLClient client)
        {
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
            var graphQLResponse = await client.PostAsync(request);
            var samples = graphQLResponse.GetDataFieldAs<Repositories>("search");
            return samples.nodes;
        }

        /// <summary>
        /// Uses client object and sampleName passed inthe url to return the sample's dependencies
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sampleName"</param>
        /// <returns> A list of dependencies. </returns>
        public static async Task<IEnumerable<DependenciesNode>> GetDependencies(GraphQLClient client, string sampleName)
        {
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
                                            }
                                        }
                                    }
                                }
                            }                        
                    }
                }",
                Variables = new { sample = sampleName }
            };

            var graphQLResponse = await client.PostAsync(request);
            var samples = graphQLResponse.GetDataFieldAs<Organization>("organization");
            var dependencies = samples.repository.dependencyGraphManifests.nodes.SelectMany(n => n.dependencies.nodes);
            return dependencies;
        }

        /// <summary>
        /// Gets languages list from parsed yaml header
        /// </summary>
        /// <param name="sampleName"></param>
        /// <returns> A new list of languages after parsing the yaml header.</returns>
        public static async Task<List<String>> GetLanguages(string sampleName)
        {
            string header = await GetYamlHeader(sampleName);
            if (!string.IsNullOrEmpty(header))
            {
                string[] lines = header.Split("\r\n");
                return SearchTerm("languages", lines);
            }
            return new List<string>();
        }

        /// <summary>
        /// Get services list from the parsed yaml header
        /// </summary>
        /// <param name="sampleName"></param>
        /// <returns> A new list of services in the yaml header after parsing it.</returns>
        public static async Task<List<string>> GetFeatures(string sampleName)
        {
            string header = await GetYamlHeader(sampleName);
            if (!string.IsNullOrEmpty(header))
            {
                string[] lines = header.Split("\r\n");
                return SearchTerm("services", lines);
            }
            return new List<string>();
        }

        /// <summary>
        /// fetch yaml header from sample repo and parse it
        /// </summary>
        /// <param name="sampleName"></param>
        /// <returns> The yaml header. </returns>
        private static async Task<string> GetYamlHeader(string sampleName)
        {
            HttpClient httpClient = new HttpClient();
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
        /// <param name="term"></param>
        /// <param name="lines"></param>
        /// <returns>A list of the searchterm specified.</returns>
        private static List<string> SearchTerm(string term, string[] lines)
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
