// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SamplesDashboard.Extensions;
using SamplesDashboard.Models;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class contains repository query services and functions
    /// to be used by the repositories API
    /// </summary>
    public class RepositoriesService
    {
        private readonly GraphQLHttpClient _graphQLClient;
        private readonly IHttpClientFactory _clientFactory;
        private readonly GitHubAuthService _gitHubAuthService;
        private readonly ManifestFromFileService _manifestFromFileService;
        private readonly MicrosoftOpenSourceService _msOpenSourceService;
        private readonly NuGetService _nuGetService;
        private readonly NpmService _npmService;
        private readonly CocoaPodsService _cocoaPodsService;
        private readonly MavenService _mavenService;
        private readonly IConfiguration _configuration;
        private readonly CacheService _cacheService;
        private readonly ILogger<RepositoriesService> _logger;
        private static Regex localizedRepos = new Regex(@".[a-z]{2}-[A-Z]{2}");

        public RepositoriesService(
            GraphQLHttpClient graphQLClient,
            IHttpClientFactory clientFactory,
            GitHubAuthService gitHubAuthService,
            ManifestFromFileService manifestFromFileService,
            MicrosoftOpenSourceService msOpenSourceService,
            NuGetService nuGetService,
            NpmService npmService,
            CocoaPodsService cocoaPodsService,
            MavenService mavenService,
            IConfiguration configuration,
            CacheService cacheService,
            ILogger<RepositoriesService> logger)
        {
            _graphQLClient = graphQLClient;
            _clientFactory = clientFactory;
            _gitHubAuthService = gitHubAuthService;
            _manifestFromFileService = manifestFromFileService;
            _msOpenSourceService = msOpenSourceService;
            _nuGetService = nuGetService;
            _npmService = npmService;
            _cocoaPodsService = cocoaPodsService;
            _mavenService = mavenService;
            _configuration = configuration;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of repositories using a GraphQL query
        /// </summary>
        /// <param name="names">The values to match in the name field</param>
        /// <param name="endCursor">A cursor to start the query from. Used in paging.</param>
        /// <returns>A list of repositories that match the names parameter</returns>
        public async Task<List<Repository>> GetRepositoriesAsync(string names)
        {
            var cacheKey = $"{names}-list";
            if (!_cacheService.TryGetValue(cacheKey, out List<string> repoList))
            {
                var endCursor = string.Empty;
                var organization = _configuration.GetValue<string>(Constants.GitHubOrg);
                repoList = new List<string>();
                do
                {
                    var cursorString = string.IsNullOrEmpty(endCursor) ? "" : $", after:\"{endCursor}\"";
                    // Make a GraphQL query to get most of the needed information in one request
                    var request = new GraphQLRequest
                    {
                        Query = $@"{{
                            search(query: ""org:{organization} {names} in:name archived:false is:public fork:true"", type: REPOSITORY, first: 100{cursorString}) {{
                                nodes {{
                                    ... on Repository {{
                                        name
                                    }}
                                }}
                                pageInfo {{
                                    endCursor
                                    hasNextPage
                                }}
                            }}
                        }}"
                    };

                    var graphQLResponse = await _graphQLClient.SendQueryAsync<GitHubGraphQLResponse>(request);

                    repoList.AddRange(graphQLResponse.Data.Search.Results.Select(r => r.Name).ToList());

                    endCursor = graphQLResponse.Data.Search.PageInfo.HasNextPage ?
                        graphQLResponse.Data.Search.PageInfo.EndCursor : string.Empty;
                }
                while(!string.IsNullOrEmpty(endCursor));

                _cacheService.Set(cacheKey, repoList);
            }

            var repositories = await GenerateRepositoryListAsync(repoList);

            return repositories;
        }

        public async Task<Repository> GetRepositoryAsync(string name)
        {
            var gitHubClient = await _gitHubAuthService.GetAuthenticatedClient();

            return await GetRepositoryAsync(name, gitHubClient);
        }

        internal async Task<Repository> GetRepositoryAsync(string name, Octokit.GitHubClient gitHubClient)
        {
            _logger.LogInformation($"Getting {name}");
            if (!_cacheService.TryGetValue(name, out Repository repository))
            {
                _logger.LogInformation($"{name} not in cache - fetching");
                var organization = _configuration.GetValue<string>(Constants.GitHubOrg);

                var request = new GraphQLRequest
                {
                    Query = @"query repo($repo: String!, $organization: String!) {
                        organization(login: $organization) {
                            repository(name: $repo) {
                                name
                                description
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
                                defaultBranchRef {
                                    name
                                }
                                updatedAt
                                vulnerabilityAlerts(first: 30) {
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
                                                repository {
                                                    name
                                                    releases (last: 1) {
                                                        nodes {
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
                    Variables = new
                    {
                        repo = name,
                        organization= organization
                    }
                };

                var graphQLResponse = await _graphQLClient.SendQueryAsync<GitHubGraphQLResponse>(request);

                repository = await GenerateRepositoryAsync(graphQLResponse.Data.Organization.Repository, organization, gitHubClient);

                _cacheService.Set(name, repository);
            }

            return repository;
        }

        private async Task<List<Repository>> GenerateRepositoryListAsync(List<string> repoList)
        {
            // Filter out localized repos
            foreach (var repo in repoList.ToList())
            {
                if (localizedRepos.IsMatch(repo) ||
                    repo == "microsoft-graph-training" ||
                    repo == "msgraph-samples-dashboard")
                {
                    _logger.LogInformation($"Removing repo {repo} from list");
                    repoList.Remove(repo);
                }
            }

            var gitHubClient = await _gitHubAuthService.GetAuthenticatedClient();

            var repositoryList = new List<Repository>();

            // Limit to 10 in parallel to reduce GitHub abuse errors
            var concurrency = _configuration.GetValue<int>(Constants.GitHubConcurrency);
            for (int i = 0; i < repoList.Count; i+=concurrency)
            {
                var processList = repoList.Skip(i).Take(concurrency);
                var taskList = (from repo in processList select GetRepositoryAsync(repo, gitHubClient)).ToList();
                var processedRepos = await Task.WhenAll(taskList);
                repositoryList.AddRange(processedRepos);
            }

            return repositoryList;
        }

        private async Task<Repository> GenerateRepositoryAsync(
            GitHubGraphQLRepoData data,
            string organization,
            Octokit.GitHubClient gitHubClient)
        {
            _logger.LogInformation($"Processing {data.Name}...");

            var repository = new Repository(data);
            // Get YAML header if present
            var yamlHeader = await GetYamlHeader(repository.Name, repository.DefaultBranch);

            // Add info from header
            repository.Language = yamlHeader?.LanguagesString ?? string.Empty;
            repository.FeatureArea = yamlHeader?.ServicesString ?? string.Empty;
            repository.Views = await FetchViews(gitHubClient, repository.Name, organization);
            repository.Owners = await FetchOwners(gitHubClient, repository.Name, organization);

            // Add dependency data
            await GetDependencyDataAsync(repository, yamlHeader, data);

            return repository;
        }

        /// <summary>
        /// Creates a github client to make calls to the API and access traffic view data
        /// </summary>
        /// <param name="githubclient"></param>
        /// <param name="repoName"></param>
        /// <param name="owner"></param>
        /// <returns>View count</returns>
        internal async Task<int> FetchViews(Octokit.GitHubClient githubclient, string repoName, string owner)
        {
            //use client to fetch views
            try
            {
                var views = await githubclient.Repository.Traffic.GetViews(
                    owner, repoName, new Octokit.RepositoryTrafficRequest(Octokit.TrafficDayOrWeek.Week));
                return views?.Count ?? 0;
            }
            catch (Octokit.AbuseException ex)
            {
                await Task.Delay(ex.RetryAfterSeconds ?? 30 * 1000);
                // Retry
                var views = await githubclient.Repository.Traffic.GetViews(
                    owner, repoName, new Octokit.RepositoryTrafficRequest(Octokit.TrafficDayOrWeek.Week));
                return views?.Count ?? 0;
            }
        }

        /// <summary>
        /// Uses Microsoft Open Source portal to get owners, falls back to FetchContributors if not configured
        /// </summary>
        /// <param name="githubclient"></param>
        /// <param name="repoName"></param>
        /// <param name="owner"></param>
        /// <returns>a list of contributors</returns>
        internal async Task<List<RepoOwner>> FetchOwners(Octokit.GitHubClient githubclient, string repoName, string owner)
        {
            var maintainerStatus = await _msOpenSourceService.GetMicrosoftMaintainers(owner, repoName);

            if (maintainerStatus != null)
            {
                if (maintainerStatus.Maintainers.Individuals.Count > 0)
                {
                    var owners = new List<RepoOwner>();

                    foreach(var user in maintainerStatus.Maintainers.Individuals)
                    {
                        owners.Add(new RepoOwner
                        {
                            DisplayName = user.DisplayName,
                            Url= $"mailto:{user.Mail}"
                        });
                    }

                    if (!string.IsNullOrEmpty(maintainerStatus.Maintainers.SecurityGroupMail))
                    {
                        owners.Add(new RepoOwner
                        {   DisplayName = maintainerStatus.Maintainers.SecurityGroupMail,
                            Url = $"mailto:{maintainerStatus.Maintainers.SecurityGroupMail}"
                        });
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
        internal async Task<List<RepoOwner>> FetchContributors(Octokit.GitHubClient githubclient, string repoName, string owner)
        {
            IReadOnlyList<Octokit.RepositoryContributor> contributors= null;
            try
            {
                contributors = await githubclient.Repository.GetAllContributors(owner, repoName);
            }
            catch (Octokit.AbuseException ex)
            {
                await Task.Delay(ex.RetryAfterSeconds ?? 30 * 1000);
                // Retry
                contributors = await githubclient.Repository.GetAllContributors(owner, repoName);
            }

            var owners = new List<RepoOwner>();
            foreach (var contributor in contributors)
            {
                if (owners.Count >= 2)
                {
                    break;
                }

                if (contributor.Login.Contains("[bot]"))
                {
                    continue;
                }

                owners.Add(new RepoOwner
                {
                    DisplayName = contributor.Login,
                    Url = contributor.HtmlUrl
                });
            }

            return owners;
        }

        internal async Task GetDependencyDataAsync(
            Repository repository,
            YamlHeader yamlHeader,
            GitHubGraphQLRepoData data)
        {
            repository.RepositoryStatus =
                repository.GraphStatus =
                repository.IdentityStatus = DependencyStatus.Unknown;

            // Get YAML header if present
            if (yamlHeader != null && yamlHeader.NoDependencies)
            {
                repository.RepositoryStatus =
                    repository.GraphStatus =
                    repository.IdentityStatus = DependencyStatus.UpToDate;
                return;
            }

            repository.SecurityAlerts = data.VulnerabilityAlerts?.TotalCount ?? 0;

            if (data.DependencyManifests.Values.Count <= 0 &&
                yamlHeader != null &&
                !string.IsNullOrEmpty(yamlHeader.DependencyFile))
            {
                // Build manifest from dependency file
                var manifest = await _manifestFromFileService
                    .BuildDependencyManifestFromFile(
                        repository.Name, repository.DefaultBranch, yamlHeader.DependencyFile);

                data.DependencyManifests.Values.Add(manifest);
            }

            var dependencyList = new List<Dependency>();

            foreach (var manifest in data.DependencyManifests.Values)
            {
                var dependencies = manifest.Dependencies.Values;
                foreach (var dependency in dependencies)
                {
                    var newDependency = new Dependency
                    {
                        PackageName = dependency.PackageName,
                        ManifestFile = manifest.FileName,
                        CurrentVersion = dependency.Requirements.NormalizeRequirementsString()
                    };

                    if (!string.IsNullOrEmpty(newDependency.CurrentVersion))
                    {
                        // Determine latest version
                        switch (dependency.PackageManager)
                        {
                            case Constants.Nuget:
                                newDependency.LatestVersion = await
                                    _nuGetService.GetLatestVersion(newDependency.PackageName,
                                        newDependency.CurrentVersion);
                                break;
                            case Constants.Npm:
                                newDependency.LatestVersion = await
                                    _npmService.GetLatestVersion(newDependency.PackageName);
                                break;
                            case Constants.Gradle:
                            case Constants.Maven:
                                var latestVersion = await
                                    _mavenService.GetLatestVersion(newDependency.PackageName,
                                        newDependency.CurrentVersion);

                                if (string.IsNullOrEmpty(latestVersion))
                                {
                                    // Fall back to GitHub's supplied value
                                    latestVersion = dependency.Repository?.Releases?.Values?.FirstOrDefault()?.TagName;
                                }

                                newDependency.LatestVersion = latestVersion;
                                break;
                            case Constants.CocoaPods:
                                newDependency.LatestVersion = await
                                    _cocoaPodsService.GetLatestVersion(newDependency.PackageName);
                                break;
                            default:
                                newDependency.LatestVersion = dependency.Repository?.Releases?.Values?.FirstOrDefault()?.TagName ?? string.Empty;
                                break;
                        }
                    }

                    newDependency.CalculateStatus(data.VulnerabilityAlerts?.Values);

                    if (newDependency.Status > repository.RepositoryStatus)
                    {
                        repository.RepositoryStatus = newDependency.Status;
                    }

                    if (IsIdentityLibrary(newDependency.PackageName) && newDependency.Status > repository.IdentityStatus)
                    {
                        repository.IdentityStatus = newDependency.Status;
                    }

                    if (IsGraphSdk(newDependency.PackageName) && newDependency.Status > repository.GraphStatus)
                    {
                        repository.GraphStatus = newDependency.Status;
                    }

                    dependencyList.Add(newDependency);
                }
            }

            repository.Dependencies = dependencyList;
        }

        /// <summary>
        /// fetch yaml header from repo repo and parse it
        /// </summary>
        /// <param name="repoName">The name of each repo</param>
        /// <returns> The yaml header. </returns>
        internal async Task<YamlHeader> GetYamlHeader(string repoName, string branch)
        {
            //downloading the yaml file
            var gitHubOrg = _configuration.GetValue<string>("GitHubOrg");
            var httpClient = _clientFactory.CreateClient("Default");
            return await YamlHeader.GetFromRepo(httpClient, gitHubOrg, repoName, branch);
        }

        internal bool IsIdentityLibrary(string packageName)
        {
            return Constants.IdentityLibraries
                .Contains(packageName.ToLower());
        }

        internal bool IsGraphSdk(string packageName)
        {
            return Constants.GraphSdks
                .Contains(packageName.ToLower());
        }
    }
}