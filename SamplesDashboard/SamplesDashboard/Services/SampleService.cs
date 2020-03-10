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
using SamplesDashboard.Models;
using Semver;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace SamplesDashboard.Services
{
    /// <summary>
    ///  This class contains samples query services and functions to be used by the samples API
    /// </summary>
    
    public class SampleService
    {
        private readonly GraphQLHttpClient _graphQlClient;
        private readonly IHttpClientFactory _clientFactory;
        private readonly NugetService _nugetService;
        private readonly NpmService _npmService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public SampleService(
            GraphQLHttpClient graphQlClient, 
            IHttpClientFactory clientFactory, IMemoryCache memoryCache, 
            IConfiguration config, 
            NugetService nugetService,
            NpmService npmService)
        {
            _graphQlClient = graphQlClient;
            _clientFactory = clientFactory;
            _cache = memoryCache;
            _config = config;
            _nugetService = nugetService;
            _npmService = npmService;
        }

        /// <summary>
        /// Gets the client object used to run the samples query and return the samples list 
        /// </summary>
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
                                    url
                                    forks {
                                        totalCount
                                    }
                                }
                            }
                        }
                }"
            };
            var graphQLResponse = await _graphQlClient.SendQueryAsync<Data>(request);

            // Fetch yaml headers and compute header values in parallel
            List<Task> TaskList = new List<Task>();
            foreach (var sampleItem in graphQLResponse?.Data?.Search.Nodes)
            {
                Task headerTask = SetHeadersAndStatus(sampleItem);
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
        private async Task SetHeadersAndStatus(Node sampleItem) 
        {
            var headerDetails = await GetHeaderDetails(sampleItem.Name);
            sampleItem.Language = headerDetails.GetValueOrDefault("languages");
            sampleItem.FeatureArea = headerDetails.GetValueOrDefault("services");

            if (!_cache.TryGetValue(sampleItem.Name, out Repository repository)) 
            {
                repository = await GetRepository(sampleItem.Name);
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>("timeout")));
                _cache.Set(sampleItem.Name, repository, cacheEntryOptions);
            }
            sampleItem.SampleStatus = repository.highestStatus;
            
        }
        
        /// <summary>
        /// Uses client object and sampleName passed inthe url to return the sample's dependencies
        /// </summary>
        /// <param name="sampleName">The name of that sample</param>
        /// <returns> A list of dependencies. </returns>
        public async Task<Repository> GetRepository(string sampleName)
        {
            //request to fetch sample dependencies
            var request = new GraphQLRequest
            {
                Query = @"query Sample($sample: String!){
                        organization(login: ""microsoftgraph"") {
                            repository(name: $sample){
                                url
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

            var graphQLResponse = await _graphQlClient.SendQueryAsync<Data>(request);
            var repository =await UpdateRepositoryStatus(graphQLResponse.Data.Organization.Repository);
            return repository;
        }

        /// <summary>
        /// Gets the repository's details and updates the status field in the dependencies
        /// </summary>
        /// <param name="repository"> A Repository object</param>
        /// <returns>An updated repository object with the status field.</returns>
        internal async Task<Repository> UpdateRepositoryStatus(Repository repository)
        {
            var dependencyGraphManifests = repository?.DependencyGraphManifests?.Nodes;
            if (dependencyGraphManifests == null) 
                return repository;
            
            // Go through the various dependency manifests in the repo
            foreach (var dependencyManifest in dependencyGraphManifests)
            {
                var dependencies = dependencyManifest?.Dependencies?.Nodes;
                if (dependencies == null) 
                    continue;
                PackageStatus highestStatus = PackageStatus.Unknown;
                // Go through each dependency in the dependency manifest
                foreach (var dependency in dependencies)
                {
                    var currentVersion = dependency.requirements;
                    if (string.IsNullOrEmpty(currentVersion)) continue;

                    //getting latest versions from the respective packagemanagers, and the default values from github
                    string latestVersion;                    
                    switch (dependency.packageManager)
                    {
                        case "NUGET":
                            latestVersion = await _nugetService.GetLatestPackageVersion(dependency.packageName);
                            break;

                        case "NPM":
                            latestVersion = await _npmService.GetLatestVersion(dependency.packageName);
                            break;

                        default:
                            latestVersion = dependency.repository?.releases?.nodes?.FirstOrDefault()?.tagName;
                            break;
                    }

                    dependency.latestVersion = latestVersion;
                    dependency.status = CalculateStatus(currentVersion.Substring(2), latestVersion);
                }
                //getting the highest status from a dependency node
                highestStatus = HighestStatus(dependencies);

                //comparing the highest statuses from different nodes
                if (highestStatus > repository.highestStatus)
                {
                    repository.highestStatus = highestStatus;
                }
            }
            return repository;
        }
        /// <summary>
        /// Calculate the status of a sample
        /// </summary>
        /// <param name="sampleVersion">The current version of the sample</param>
        /// <param name="latestVersion">The latest version of the sample</param>
        /// <returns><see cref="PackageStatus"/> of the sample Version </returns>
        internal PackageStatus CalculateStatus(string sampleVersion, string latestVersion)
        {
            if (string.IsNullOrEmpty(sampleVersion) || string.IsNullOrEmpty(latestVersion))
                return PackageStatus.Unknown;

            // Dropping any 'v's that occur before the version
            if (sampleVersion.StartsWith("v"))
            {
                sampleVersion = sampleVersion.Substring(1);
            }
            if (latestVersion.StartsWith("v"))
            {
                latestVersion = latestVersion.Substring(1);
            }

            //If version strings are equal, they are upto date
            if (sampleVersion.Equals(latestVersion))
                return PackageStatus.UpToDate;

            // Try to parse the versions into SemVersion objects
            if (!SemVersion.TryParse(sampleVersion.Trim(), out SemVersion sample) ||
                !SemVersion.TryParse(latestVersion.Trim(), out SemVersion latest)) 
            {
                //Unable to determine the versions
                return PackageStatus.Unknown;
            }

            int status = sample.CompareTo(latest);

            if (status == 0)
            {
                //Version objects are the same so packages are upto date
                return PackageStatus.UpToDate;
            }
            else if (sample.Major == latest.Major && sample.Minor == latest.Minor)
            {
                //Difference is only in the build version therefore package requires an urgent update 
                return PackageStatus.UrgentUpdate;
            }
            else if (status < 0)
            {
                //Difference is in the major and/or minor version and the sample version is behind the latest version
                return PackageStatus.Update;
            }
            else
            {
                //Unable to determine the versions
                return PackageStatus.Unknown;
            }
        }
        /// <summary>
        /// Get dependency statuses from a sample and return the highest status
        /// </summary>
        /// <param name="dependencies"> Dependencies in a sample</param>
        /// <returns><see cref="PackageStatus"/>The highest PackageStatus from dependencies</returns>
        private PackageStatus HighestStatus(DependenciesNode[] dependencies)
        {
                PackageStatus[] statuses = dependencies.Select(dependency => dependency.status).ToArray();
                return statuses.Max();
        }
        /// <summary>
        /// Get header details list from the parsed yaml header
        /// </summary>
        /// <param name="sampleName">The name of each sample</param>
        /// <returns> A new list of services in the yaml header after parsing it.</returns>
        internal async Task<Dictionary<string,string>> GetHeaderDetails(string sampleName)
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
            //downloading the yaml file
            var httpClient = _clientFactory.CreateClient();
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
