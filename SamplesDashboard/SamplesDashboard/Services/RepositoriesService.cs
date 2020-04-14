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
using Semver;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SamplesDashboard.Models;
using System.IO;
using Newtonsoft.Json;

namespace SamplesDashboard.Services
{
    /// <summary>
    ///  This class contains repository query services and functions to be used by the repositories API
    /// </summary>    
    public class RepositoriesService
    {
        private readonly GraphQLHttpClient _graphQlClient;
        private readonly IHttpClientFactory _clientFactory;
        private readonly NugetService _nugetService;
        private readonly NpmService _npmService;
        private readonly AzureSdkService _azureSdkService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;



        public RepositoriesService(
            GraphQLHttpClient graphQlClient,
            IHttpClientFactory clientFactory,
            NugetService nugetService,
            NpmService npmService,
            AzureSdkService azureSdkService,
            IMemoryCache memoryCache,
            IConfiguration config)
        {
            _graphQlClient = graphQlClient;
            _clientFactory = clientFactory;
            _nugetService = nugetService;
            _npmService = npmService;
            _azureSdkService = azureSdkService;
            _cache = memoryCache;
            _config = config;
        }

        /// <summary>
        /// Gets the client object used to run the repos query and return the repos list 
        /// </summary>
        /// <returns> A list of repos.</returns>
        public async Task<List<Node>> GetRepositories(string name, string endCursor = null)
        {
            // Request to fetch the list of repos for graph
            string cursorString = "";

            if (!string.IsNullOrEmpty(endCursor))
            {
                cursorString = $", after:\"{endCursor}\"";
            }           

            var request = new GraphQLRequest
            {
                Query = @"
	            {              
                  search(query: ""org:microsoftgraph"+ $"{name}"+ @" in:name archived:false"", type: REPOSITORY, first: 100 "+ $"{cursorString}" + @" ) {
                        nodes {
                                ... on Repository {
                                    name
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
                                    collaborators(first: 10) {
                                        edges {
                                            permission
                                            node {
                                                login
                                                name
                                                url
                                            }
                                        }
                                    }
                                    url
                                    forks {
                                        totalCount
                                    }
                                }
                            }
                        pageInfo {
                              endCursor
                              hasNextPage
                           }                       
                    }
                }"
            };
            var graphQLResponse = await _graphQlClient.SendQueryAsync<Data>(request);          

            // Fetch yaml headers and compute header values in parallel
            List<Task> TaskList = new List<Task>();
            foreach (var repoItem in graphQLResponse?.Data?.Search.Nodes)
            {
                Task headerTask = SetHeadersAndStatus(repoItem);
                TaskList.Add(headerTask);
            } 

            await Task.WhenAll(TaskList);
            //returning a list of only repos with dependencies
            var repos = graphQLResponse?.Data?.Search.Nodes.Where(nodeItem => (nodeItem.HasDependendencies == true)).ToList();           

            //Taking the next 100 repos(paginating using endCursor object)
            var hasNextPage = graphQLResponse?.Data?.Search.PageInfo.HasNextPage;
            endCursor = graphQLResponse?.Data?.Search.PageInfo.EndCursor;

            if (hasNextPage == true)
            {
                var nextRepos = await GetRepositories(name, endCursor);
                repos.AddRange(nextRepos);
            }          

            return repos;            
        }
        /// <summary>
        /// Creates a github client to make calls to the API and access traffic view data
        /// </summary>
        /// <param name="repoName"></param>
        /// <returns>View count</returns>
        internal async Task<int?> FetchViews(string repoName)
        {
            ViewData views;

            if (!_cache.TryGetValue(repoName, out views))
            {
                var httpClient = _clientFactory.CreateClient("github");
                
                HttpResponseMessage response = await httpClient.GetAsync(string.Concat("repos/microsoftgraph/", repoName, "/traffic/views"));
                if (response.IsSuccessStatusCode)
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var streamReader = new StreamReader(responseStream))
                    using (var jsonTextReader = new JsonTextReader(streamReader))
                    {
                        var serializer = new JsonSerializer();
                        views = serializer.Deserialize<ViewData>(jsonTextReader);

                        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));
                        _cache.Set(repoName, views, cacheEntryOptions);
                    }
                      
                }                  
              
            }

            if (views == null)
            {
                return null;
            }
            return views.Count;
        }

        /// <summary>
        ///Getting header details and setting the language and featureArea items        
        /// </summary>
        /// <param name="repoItem">A specific repo item from the repos list</param>
        /// <returns> A list of repos.</returns>
        private async Task SetHeadersAndStatus(Node repoItem) 
        {            
            Repository repository;
            if (!_cache.TryGetValue(repoItem.Name, out repository))
            {
                repository = await GetRepository(repoItem.Name);            
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));
                _cache.Set(repoItem.Name, repository, cacheEntryOptions);
            }

            if (repository?.DependencyGraphManifests?.Nodes?.Length > 0)
            {
                repoItem.HasDependendencies = true;
                repoItem.RepositoryStatus = repository.highestStatus;
                var headerDetails = await GetHeaderDetails(repoItem.Name);
                repoItem.Language = headerDetails.GetValueOrDefault("languages");
                repoItem.FeatureArea = headerDetails.GetValueOrDefault("services");
                repoItem.Views = await FetchViews(repoItem.Name);

                if(repoItem.Collaborators != null)
                {                 
                    var userName = repoItem.Collaborators.Edges.Where(p => p.Permission == "ADMIN")
                                                              .Select (p=> new { p.Node.Name, p.Node.Url})
                                                              .ToDictionary(p=>p.Name, p=>p.Url);
                    
                    repoItem.OwnerProfiles = userName;  

                }
            } 
        }
        
        /// <summary>
        /// Uses client object and repoName passed inthe url to return the repo's dependencies
        /// </summary>
        /// <param name="repoName">The name of that repo</param>
        /// <returns> A list of dependencies. </returns>
        public async Task<Repository> GetRepository(string repoName)
        {
            //request to fetch repo dependencies
            var request = new GraphQLRequest
            {
                Query = @"query repo($repo: String!){
                        organization(login: ""microsoftgraph"") {
                            repository(name: $repo){
                                description
                                url
                                dependencyGraphManifests(withDependencies: true) {
                                    nodes {
                                        filename
                                        dependencies(first: 100) {
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
                Variables = new { repo = repoName }
            };

            var graphQLResponse = await _graphQlClient.SendQueryAsync<Data>(request); 
           
            var repository = await UpdateRepositoryStatus(graphQLResponse.Data?.Organization.Repository);           
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

                    //getting latest versions from the respective packagemanagers,azure sdks and the default values from github
                    string latestVersion;
                    string azureSdkVersion = String.Empty;
                    switch (dependency.packageManager)
                    {
                        case "NUGET":
                            latestVersion = await _nugetService.GetLatestPackageVersion(dependency.packageName);
                            azureSdkVersion = await _azureSdkService.GetAzureSdkVersions(dependency.packageName);                         
                            break;

                        case "NPM":
                            latestVersion = await _npmService.GetLatestVersion(dependency.packageName);
                            break;

                        default:
                            latestVersion = dependency.repository?.releases?.nodes?.FirstOrDefault()?.tagName;
                            break;
                    }

                    dependency.latestVersion = latestVersion;
                    dependency.azureSdkVersion = azureSdkVersion;
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
        /// Calculate the status of a repo
        /// </summary>
        /// <param name="repoVersion">The current version of the repo</param>
        /// <param name="latestVersion">The latest version of the repo</param>
        /// <returns><see cref="PackageStatus"/> of the repo Version </returns>
        internal PackageStatus CalculateStatus(string repoVersion, string latestVersion)
        {
            if (string.IsNullOrEmpty(repoVersion) || string.IsNullOrEmpty(latestVersion))
                return PackageStatus.Unknown;

            // Dropping any 'v's that occur before the version
            if (repoVersion.StartsWith("v"))
            {
                repoVersion = repoVersion.Substring(1);
            }
            if (latestVersion.StartsWith("v"))
            {
                latestVersion = latestVersion.Substring(1);
            }

            //If version strings are equal, they are upto date
            if (repoVersion.Equals(latestVersion))
                return PackageStatus.UpToDate;

            // Try to parse the versions into SemVersion objects
            if (!SemVersion.TryParse(repoVersion.Trim(), out SemVersion repo) ||
                !SemVersion.TryParse(latestVersion.Trim(), out SemVersion latest)) 
            {
                //Unable to determine the versions
                return PackageStatus.Unknown;
            }

            int status = repo.CompareTo(latest);

            if (status == 0)
            {
                //Version objects are the same so packages are upto date
                return PackageStatus.UpToDate;
            }
            else if (repo.Major == latest.Major && repo.Minor == latest.Minor)
            {
                //Difference is only in the build version therefore package requires an urgent update 
                return PackageStatus.UrgentUpdate;
            }
            else if (status < 0)
            {
                //Difference is in the major and/or minor version and the repo version is behind the latest version
                return PackageStatus.Update;
            }
            else
            {
                //Unable to determine the versions
                return PackageStatus.Unknown;
            }
        }

        /// <summary>
        /// Get dependency statuses from a repo and return the highest status
        /// </summary>
        /// <param name="dependencies"> Dependencies in a repo</param>
        /// <returns><see cref="PackageStatus"/>The highest PackageStatus from dependencies</returns>
        private PackageStatus HighestStatus(DependenciesNode[] dependencies)
        {
            PackageStatus[] statuses = dependencies.Select(dependency => dependency.status).ToArray();
            if (statuses.Any())
            {
                return statuses.Max();
            }
            return PackageStatus.Unknown;
        }

        /// <summary>
        /// Get header details list from the parsed yaml header
        /// </summary>
        /// <param name="repoName">The name of each repo</param>
        /// <returns> A new list of services in the yaml header after parsing it.</returns>
        internal async Task<Dictionary<string,string>> GetHeaderDetails(string repoName)
        {
            string header = await GetYamlHeader(repoName);
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
        /// fetch yaml header from repo repo and parse it
        /// </summary>
        /// <param name="repoName">The name of each repo</param>
        /// <returns> The yaml header. </returns>
        private async Task<string> GetYamlHeader(string repoName)
        {
            //downloading the yaml file
            var httpClient = _clientFactory.CreateClient();
            HttpResponseMessage responseMessage = await httpClient.GetAsync(string.Concat("https://raw.githubusercontent.com/microsoftgraph/", repoName, "/master/README.md"));

            if (responseMessage.StatusCode.ToString().Equals("NotFound"))
                responseMessage = await httpClient.GetAsync(string.Concat("https://raw.githubusercontent.com/microsoftgraph/", repoName, "/master/Readme.md"));

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
