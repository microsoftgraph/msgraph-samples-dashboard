// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System;
using System.Text;
using GraphQL;
using GraphQL.Client.Http;
using Semver;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SamplesDashboard.Models;
using Octokit;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
        private readonly MavenService _mavenService;
        private readonly CocoaPodsService _cocoaPodsService;
        private readonly AzureSdkService _azureSdkService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger<RepositoriesService> _logger;
        private readonly GithubAuthService _githubAuthService;
        private readonly MicrosoftOpenSourceService _msOpenSourceService;

        public RepositoriesService(
            GraphQLHttpClient graphQlClient,
            IHttpClientFactory clientFactory,
            NugetService nugetService,
            NpmService npmService,
            MavenService mavenService,
            CocoaPodsService cocoaPodsService,
            AzureSdkService azureSdkService,
            GithubAuthService githubAuthService,
            MicrosoftOpenSourceService msOpenSourceService,
            IMemoryCache memoryCache,
            IConfiguration config,
            ILogger<RepositoriesService> logger)
        {
            _graphQlClient = graphQlClient;
            _clientFactory = clientFactory;
            _nugetService = nugetService;
            _npmService = npmService;
            _mavenService = mavenService;
            _cocoaPodsService = cocoaPodsService;
            _azureSdkService = azureSdkService;
            _cache = memoryCache;
            _config = config;
            _logger = logger;
            _githubAuthService = githubAuthService;
            _msOpenSourceService = msOpenSourceService;
        }

        /// <summary>
        /// Gets the client object used to run the repos query and return the repos list
        /// </summary>
        /// <returns> A list of repos.</returns>
        public async Task<List<Node>> GetRepositories(string names, string endCursor = null)
        {
            // Request to fetch the list of repos for graph
            var cursorString = "";

            if (!string.IsNullOrEmpty(endCursor))
            {
                cursorString = $", after:\"{endCursor}\"";
            }

            var request = new GraphQLRequest
            {
                Query = @"
                {
                  search(query: ""org:microsoftgraph" + $" {names}" + @" in:name archived:false is:public fork:true"", type: REPOSITORY, first: 5 " + $"{cursorString}" + @" ) {
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
                                    defaultBranchRef {
                                        name
                                    },
                                    pushedAt
                                }
                            }
                        pageInfo {
                              endCursor
                              hasNextPage
                           }
                    }
                }"
            };

            var graphQlResponse = await _graphQlClient.SendQueryAsync<Data>(request);

            // get githubapp token
            var token = await _githubAuthService.GetGithubAppToken();

            // Create a new GitHubClient using the installation token as authentication
            var githubClient = new GitHubClient(new ProductHeaderValue(_config.GetValue<string>("product")))
            {
                Credentials = new Credentials(token)
            };

            //returning a list of all repos
            var repositories = graphQlResponse?.Data?.Search.Nodes.ToList();

            //remove localized repos to reduce repetition of samples
            foreach (var repo in repositories.ToList())
            {
                var regex = new Regex (@".[a-z]{2}-[A-Z]{2}");
                if(regex.IsMatch(repo?.Name) ||
                   repo?.Name == "microsoft-graph-training" ||
                   repo?.Name == "msgraph-samples-dashboard")
                {
                    _logger.LogInformation($"Removing repo {repo.Name} from list");
                    repositories.Remove(repo);
                }
            }

            // Fetch yaml headers and compute header values in parallel
            var taskList = (from repoItem in repositories select SetHeadersAndStatus(githubClient, repoItem, "microsoftgraph")).ToList();
            await Task.WhenAll(taskList);

            //Taking the next 100 repos(paginating using endCursor object)
            var hasNextPage = graphQlResponse?.Data?.Search.PageInfo.HasNextPage;
            endCursor = graphQlResponse?.Data?.Search.PageInfo.EndCursor;

            if (hasNextPage == true)
            {
                var nextRepos = await GetRepositories(names, endCursor);
                repositories?.AddRange(nextRepos);
            }

            return repositories;
        }

        /// <summary>
        /// Creates a github client to make calls to the API and access traffic view data
        /// </summary>
        /// <param name="githubclient"></param>
        /// <param name="repoName"></param>
        /// <param name="owner"></param>
        /// <returns>View count</returns>
        internal async Task<int?> FetchViews(GitHubClient githubclient, string repoName, string owner)
        {
            //use client to fetch views
            try
            {
                var views = await githubclient.Repository.Traffic.GetViews(owner, repoName, new RepositoryTrafficRequest(TrafficDayOrWeek.Week));
                return views?.Count;
            }
            catch (Octokit.AbuseException ex)
            {
                Task.Delay(ex.RetryAfterSeconds ?? 30 * 1000).RunSynchronously();
                // Retry
                var views = await githubclient.Repository.Traffic.GetViews(owner, repoName, new RepositoryTrafficRequest(TrafficDayOrWeek.Week));
                return views?.Count;
            }
        }

        /// <summary>
        /// Uses Microsoft Open Source portal to get owners, falls back to FetchContributors if not configured
        /// </summary>
        /// <param name="githubclient"></param>
        /// <param name="repoName"></param>
        /// <param name="owner"></param>
        /// <returns>a list of contributors</returns>
        internal async Task<Dictionary<string, string>> FetchOwners(GitHubClient githubclient, string repoName, string owner)
        {
            var maintainerStatus = await _msOpenSourceService.GetMicrosoftMaintainers(owner, repoName);

            if (maintainerStatus != null)
            {
                if (maintainerStatus.Maintainers.Individuals.Count > 0)
                {
                    var owners = new Dictionary<string, string>();

                    foreach(var user in maintainerStatus.Maintainers.Individuals)
                    {
                        owners.Add(user.DisplayName, $"mailto:{user.Mail}");
                    }

                    if (!string.IsNullOrEmpty(maintainerStatus.Maintainers.SecurityGroupMail))
                    {
                        owners.Add(maintainerStatus.Maintainers.SecurityGroupMail,
                            $"mailto:{maintainerStatus.Maintainers.SecurityGroupMail}");
                    }

                    return owners;
                }
            }

            return await FetchContributors(githubclient, repoName, owner);
        }

        /// <summary>
        /// Uses githubclient to fetch a list of contributors
        /// </summary>
        /// <param name="githubclient"></param>
        /// <param name="repoName"></param>
        /// <param name="owner"></param>
        /// <returns>a list of contributors</returns>
        internal async Task<Dictionary<string, string>> FetchContributors(GitHubClient githubclient, string repoName, string owner)
        {
            IReadOnlyList<RepositoryContributor> contributors= null;
            try
            {
                contributors = await githubclient.Repository.GetAllContributors(owner, repoName);
            }
            catch (Octokit.AbuseException ex)
            {
                Task.Delay(ex.RetryAfterSeconds ?? 30 * 1000).RunSynchronously();
                // Retry
                contributors = await githubclient.Repository.GetAllContributors(owner, repoName);
            }

            var contributorList = contributors.Select(p => new { p.Login, p.HtmlUrl }).Take(3).ToDictionary(p => p.Login, p => p.HtmlUrl);

            //Remove dependabot from the list
            if (contributorList.ContainsKey("dependabot[bot]") || contributorList.ContainsKey("dependabot-preview[bot]"))
            {
                var keysToRemove = contributorList.Where(r => r.Key == "dependabot[bot]" || r.Key == "dependabot-preview[bot]")
                                   .Select(r => r.Key).ToList();
                foreach (var key in keysToRemove)
                {
                    contributorList.Remove(key);
                }
            }
            return contributorList;
        }

        /// <summary>
        ///Getting header details and setting the language and featureArea items
        /// </summary>
        /// <param name="repoItem">A specific repo item from the repos list</param>
        /// <returns> A list of repos.</returns>
        private async Task SetHeadersAndStatus(GitHubClient githubClient, Node repoItem, string owner)
        {
            _logger.LogInformation($"Processing {repoItem.Name}...");
            if (!_cache.TryGetValue(repoItem.Name, out Repository repository))
            {
                _logger.LogInformation("Not in cache - fetching");
                repository = await GetRepository(repoItem.Name);
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.GetValue<double>(Constants.Timeout)));
                _cache.Set(repoItem.Name, repository, cacheEntryOptions);
            }

            if (repository?.DependencyGraphManifests?.Nodes?.Length == 0 || repository?.DependencyGraphManifests?.Nodes == null)
            {
                repoItem.HasDependencies = false;
            }
            else if (repository?.DependencyGraphManifests?.Nodes?.Length > 0)
            {
                repoItem.HasDependencies = true;
                repoItem.RepositoryStatus = repository.highestStatus;
                repoItem.IdentityStatus = repository.IdentityStatus;
                repoItem.GraphStatus = repository.GraphStatus;
            }

            var headerDetails = await GetHeaderDetails(repoItem.Name, repoItem.DefaultBranch?.Name ?? "master");
            repoItem.Language = headerDetails.GetValueOrDefault("languages");
            repoItem.FeatureArea = headerDetails.GetValueOrDefault("services");
            repoItem.Views = await FetchViews(githubClient, repoItem.Name, owner);
            repoItem.OwnerProfiles = await FetchOwners(githubClient, repoItem.Name, owner);
        }

        /// <summary>
        /// Uses client object and repoName passed in the url to return the repo's dependencies
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
                                name
                                description
                                url
                                vulnerabilityAlerts(first: 30)  {
                                    totalCount
                                    edges {
                                        node {
                                            securityVulnerability {
                                                package {
                                                    name
                                                }
                                            }
                                        }
                                    }
                                }
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
                                defaultBranchRef {
                                    name
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
        /// Gets a list of versions from nuget and computes the latest version
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="currentVersion"></param>
        /// <returns>latestVersion</returns>
        public async Task<string> GetLatestNugetVersion(string packageName, string currentVersion)
        {
            var nugetPackageVersions = await _nugetService.GetPackageVersions(packageName);
            var latestVersion = nugetPackageVersions.LastOrDefault()?.ToString();
            if (latestVersion == null) return latestVersion;

            //check if current version is preview, return latest version, whether preview or non-preview
            if (currentVersion.Contains("preview") && latestVersion.Contains("preview"))
            {
                return latestVersion;
            }
            //check if only latest version is preview, set to latest non-preview version
            else if (latestVersion.Contains("preview"))
            {
                var nonPreviewVersions = (from version in nugetPackageVersions where !version.ToString()
                    .Contains("preview") select version.ToString()).ToList();
                latestVersion = nonPreviewVersions.LastOrDefault();
            }
            return latestVersion;
        }

        /// <summary>
        /// Gets the repository's details and updates the status field in the dependencies
        /// </summary>
        /// <param name="repository"> A Repository object</param>
        /// <returns>An updated repository object with the status field.</returns>
        internal async Task<Repository> UpdateRepositoryStatus(Repository repository)
        {
            var headerDetails = await GetHeaderDetails(repository.Name, repository.DefaultBranch?.Name ?? "master");
            repository.IdentityStatus = PackageStatus.Unknown;
            repository.GraphStatus = PackageStatus.Unknown;
            var vulnerabilityCount = repository?.VulnerabilityAlerts?.TotalCount;
            var dependencyGraphManifests = repository?.DependencyGraphManifests?.Nodes;
            if (dependencyGraphManifests == null || dependencyGraphManifests.Count() == 0)
            {
                var dependencyFile = headerDetails.GetValueOrDefault("dependencyFile");
                if (string.IsNullOrEmpty(dependencyFile))
                {
                    return repository;
                }

                // Build dependency graph from file
                dependencyGraphManifests = await BuildDependencyGraphFromFile(repository.Name,
                    repository.DefaultBranch?.Name ?? "master", dependencyFile);

                repository.DependencyGraphManifests.Nodes = dependencyGraphManifests;
            }

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
                            latestVersion = await GetLatestNugetVersion(dependency.packageName, currentVersion);
                            azureSdkVersion = await _azureSdkService.GetAzureSdkVersions(dependency.packageName);
                            break;

                        case "NPM":
                            latestVersion = await _npmService.GetLatestVersion(dependency.packageName);
                            break;

                        case "GRADLE":
                            latestVersion = await _mavenService.GetLatestVersion(dependency.packageName);
                            break;

                        case "COCOAPODS":
                            latestVersion = await _cocoaPodsService.GetLatestVersion(dependency.packageName);
                            break;

                        default:
                            latestVersion = dependency.repository?.releases?.nodes?.FirstOrDefault()?.tagName;
                            break;
                    }
                    dependency.latestVersion = latestVersion;
                    dependency.azureSdkVersion = azureSdkVersion;

                    //calculate status normally for repos without security alerts
                    if (vulnerabilityCount == 0)
                    {
                        dependency.status = CalculateStatus(currentVersion.Substring(2), latestVersion);
                    }

                    //check if a repo has security alerts
                    else if (vulnerabilityCount > 0)
                    {
                        var librariesWithAlerts = repository?.VulnerabilityAlerts.Edges.Select(p => p.Node?.SecurityVulnerability.Package.Name);

                        //if the name of the dependency is in the list of libraries with alerts, set status to urgent update
                        if (librariesWithAlerts.Contains(dependency.packageName))
                        {
                            dependency.status = PackageStatus.UrgentUpdate;
                        }
                        else
                        {
                            dependency.status = CalculateStatus(currentVersion.Substring(2), latestVersion);
                        }
                    }

                    // Check if dependency is Identity library
                    if (IsIdentityLibrary(dependency) && dependency.status > repository.IdentityStatus)
                    {
                        repository.IdentityStatus = dependency.status;
                    }
                    // Check if dependency is Graph SDK
                    else if (IsGraphSdk(dependency) && dependency.status > repository.GraphStatus)
                    {
                        repository.GraphStatus = dependency.status;
                    }

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

            //If version strings are equal, they are up-to-date
            if (repoVersion.Equals(latestVersion))
                return PackageStatus.UpToDate;

            // Try to parse the versions into SemVersion objects
            if (!SemVersion.TryParse(repoVersion.Split(',').First().Trim(), out SemVersion repo) ||
                !SemVersion.TryParse(latestVersion.Trim(), out SemVersion latest))
            {
                //Unable to determine the versions
                return PackageStatus.Unknown;
            }

            int status = repo.CompareTo(latest);

            if (status == 0)
            {
                //Version objects are the same so packages are up-to-date
                return PackageStatus.UpToDate;
            }
            else if (repo.Major == latest.Major && repo.Minor == latest.Minor)
            {
                //Difference is only in the build version therefore package requires an urgent update
                return PackageStatus.PatchUpdate;
            }
            else if (repo.Major == latest.Major)
            {
                //Difference is in minor version and the repo version is behind the latest version
                return PackageStatus.MinorVersionUpdate;
            }
            else if (status < 0)
            {
                // Difference is in major version
                return PackageStatus.MajorVersionUpdate;
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
        internal async Task<Dictionary<string,string>> GetHeaderDetails(string repoName, string branch)
        {
            string header = await GetYamlHeader(repoName, branch);
            if (!string.IsNullOrEmpty(header))
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                var yml = deserializer.Deserialize<YamlHeader>(header);

                Dictionary<string, string> keyValuePairs = new Dictionary<string,string>{
                    { "languages", yml.Languages == null ? "" : string.Join(',', yml.Languages) },
                    { "services", yml.Extensions?.Services == null ? "" : string.Join(',', yml.Extensions.Services) },
                    { "dependencyFile", yml.DependencyFile }
                };

                return keyValuePairs;
            }
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// fetch yaml header from repo repo and parse it
        /// </summary>
        /// <param name="repoName">The name of each repo</param>
        /// <returns> The yaml header. </returns>
        private async Task<string> GetYamlHeader(string repoName, string branch)
        {
            //downloading the yaml file
            var httpClient = _clientFactory.CreateClient();
            var responseMessage = await httpClient.GetAsync(
                $"https://raw.githubusercontent.com/microsoftgraph/{repoName}/{branch}/README.md");

            if (responseMessage.StatusCode.ToString().Equals("NotFound"))
                responseMessage = await httpClient.GetAsync(
                    $"https://raw.githubusercontent.com/microsoftgraph/{repoName}/{branch}/Readme.md");

            if (responseMessage.IsSuccessStatusCode)
            {
                var fileContents = await responseMessage.Content.ReadAsStringAsync();
                var stringSeparator = new string[] { "\r\n---\r\n", "\n---\n" };
                var parts = fileContents.Split(stringSeparator, StringSplitOptions.RemoveEmptyEntries);

                //we have a valid header between ---
                if (parts.Length > 1)
                {
                    var header = parts[0];
                    if (header.StartsWith("---"))
                    {
                        header = header.Substring(3);
                    }
                    return header;
                }
            }

            // Fall back to devx.yml
            var devxResponseMessage = await httpClient.GetAsync(
                $"https://raw.githubusercontent.com/microsoftgraph/{repoName}/{branch}/devx.yml");

            if(devxResponseMessage.IsSuccessStatusCode)
            {
                return await devxResponseMessage.Content.ReadAsStringAsync();
            }

            return "";
        }

        /// <summary>
        /// Check if a dependency is an Identity library
        /// </summary>
        /// <param name="dependency">The dependency to check</param>
        /// <returns> true if dependency is an Identity library </returns>
        private bool IsIdentityLibrary(DependenciesNode dependency)
        {
            return Constants.IdentityLibraries
                .Contains(dependency.packageName.ToLower());
        }

        /// <summary>
        /// Check if a dependency is a Graph SDK
        /// </summary>
        /// <param name="dependency">The dependency to check</param>
        /// <returns> true if dependency is Graph SDk </returns>
        private bool IsGraphSdk(DependenciesNode dependency)
        {
            return Constants.GraphSdks
                .Contains(dependency.packageName.ToLower());
        }

        /// <summary>
        /// Builds a dependency graph manifest from a dependency file in the repo
        /// </summary>
        /// <param name="repoName">The name of the repo</param>
        /// <param name="defaultBranch">The default branch of the repo</param>
        /// <param name="dependencyFile">The relative path to the dependency file</param>
        /// <returns> The dependency graph manifest </returns>
        internal async Task<DependencyGraphManifestsNode[]> BuildDependencyGraphFromFile(
            string repoName, string defaultBranch, string dependencyFile)
        {
            //downloading the dependency file
            var fileType = GetSupportedFileType(dependencyFile);
            if (fileType != SupportedDependencyFileType.Unsupported)
            {
                var httpClient = _clientFactory.CreateClient();
                var responseMessage = await httpClient.GetAsync(
                    $"https://raw.githubusercontent.com/microsoftgraph/{repoName}/{defaultBranch}/{dependencyFile}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    var fileContents = await responseMessage.Content.ReadAsStringAsync();
                    var stringSeparator = new string[] { "\r\n", "\n" };
                    var lines = fileContents.Split(stringSeparator, StringSplitOptions.RemoveEmptyEntries);

                    // Handle file type
                    switch (fileType)
                    {
                        case SupportedDependencyFileType.Gradle:
                            return BuildGradleDependencies(dependencyFile, lines);
                        case SupportedDependencyFileType.PodFile:
                            return BuildPodfileDependencies(dependencyFile, lines);
                        default:
                            break;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines the dependency manager type from the file name and extension
        /// </summary>
        /// <param name="dependencyFile">The relative path to the dependency file</param>
        /// <returns> The SupportedDependencyFileType value mapped to the file type </returns>
        internal SupportedDependencyFileType GetSupportedFileType(string dependencyFile)
        {
            var fileExtension = Path.GetExtension(dependencyFile).ToLower();
            var fileName = Path.GetFileNameWithoutExtension(dependencyFile).ToLower();

            var lowered = fileExtension.ToLower();

            if (fileExtension == ".gradle") return SupportedDependencyFileType.Gradle;
            if (fileName == "podfile") return SupportedDependencyFileType.PodFile;

            return SupportedDependencyFileType.Unsupported;
        }

        /// <summary>
        /// Generates a dependency graph manifest from a Gradle file
        /// </summary>
        /// <param name="dependencyFile">The relative path to the Gradle file</param>
        /// <param name="lines">The contents of the Gradle file</param>
        /// <returns> The dependency graph manifest </returns>
        internal DependencyGraphManifestsNode[] BuildGradleDependencies(string dependencyFile, string[] lines)
        {
            var dependencies = new List<DependenciesNode>();

            foreach(var line in lines)
            {
                if (line.Trim().StartsWith("implementation '"))
                {
                    var match = Regex.Match(line, "'(.*):(.*)'");

                    if (match.Success && match.Groups.Count == 3)
                    {
                        dependencies.Add(new DependenciesNode{
                            packageManager = "GRADLE",
                            packageName = match.Groups[1].Value,
                            requirements = $"= {match.Groups[2].Value}"
                        });
                    }
                }
            }

            var manifestList = new List<DependencyGraphManifestsNode> {
                new DependencyGraphManifestsNode {
                    Filename = dependencyFile,
                    Dependencies = new Dependencies {
                        Nodes = dependencies.ToArray()
                    }
                }
            };

            return manifestList.ToArray();
        }

        /// <summary>
        /// Generates a dependency graph manifest from a Podfile
        /// </summary>
        /// <param name="dependencyFile">The relative path to the Podfile</param>
        /// <param name="lines">The contents of the Podfile</param>
        /// <returns> The dependency graph manifest </returns>
        internal DependencyGraphManifestsNode[] BuildPodfileDependencies(string dependencyFile, string[] lines)
        {
            var dependencies = new List<DependenciesNode>();

            foreach (var line in lines)
            {
                if (line.Trim().ToLower().StartsWith("pod"))
                {
                    var match = Regex.Match(line.ToLower(), @"pod\s*'(\w*)',\s*'\D*([\d\.]*)'");

                    if (match.Success && match.Groups.Count == 3)
                    {
                        dependencies.Add(new DependenciesNode{
                            packageManager = "COCOAPODS",
                            packageName = match.Groups[1].Value,
                            requirements = $"=={match.Groups[2].Value}"
                        });
                    }
                }
            }

            var manifestList = new List<DependencyGraphManifestsNode> {
                new DependencyGraphManifestsNode {
                    Filename = dependencyFile,
                    Dependencies = new Dependencies {
                        Nodes = dependencies.ToArray()
                    }
                }
            };

            return manifestList.ToArray();
        }
    }
}
