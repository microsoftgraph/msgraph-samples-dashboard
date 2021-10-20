// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using SamplesDashboard.Models;
using Semver;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class contains search.maven.org queries to be used by the samples
    /// </summary>
    public class MavenService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly CacheService _cacheService;
        private readonly JsonSerializerOptions _jsonOptions;

        public MavenService(IHttpClientFactory clientFactory, CacheService cacheService)
        {
            _clientFactory = clientFactory;
            _cacheService = cacheService;
            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        /// <summary>
        ///Creates a httpclient to query Maven's registry to get package details
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns>latest package version</returns>
        public async Task<string> GetLatestVersion(string packageName, string currentVersion)
        {
            // TODO: Make cache key a variable (change pattern in all caching services)
            var cacheKey = $"maven:{packageName}";
            if (!_cacheService.TryGetValue(cacheKey, out MavenQuery mavenData))
            {
                var httpClient = _clientFactory.CreateClient();

                var packageParts = packageName.Split(':');

                // Check Google's Maven repo first for any Android packages
                var latestVersion = await GetLatestAndroidPackageVersion(httpClient, packageParts[0], packageParts[1], currentVersion);
                if (!string.IsNullOrEmpty(latestVersion)) return latestVersion;

                var apiUrl = $"https://search.maven.org/solrsearch/select?q=g:%22{packageParts[0]}%22%20AND%20a:%22{packageParts[1]}%22&wt=json&core=gav";
                var responseMessage = await httpClient.GetAsync(apiUrl);

                if (responseMessage.IsSuccessStatusCode)
                {
                    await using var stream = await responseMessage.Content.ReadAsStreamAsync();
                    {
                        mavenData = await JsonSerializer.DeserializeAsync<MavenQuery>(stream, _jsonOptions);
                    }
                }

                if (mavenData != null)
                {
                    _cacheService.Set(cacheKey, mavenData);
                }
            }

            var availableVersions = mavenData?.Response?.Docs?.Select(p => p.Version).ToArray();

            return GetLatestVersionBasedOnCurrentVersion(availableVersions, currentVersion);
        }

        /// <summary>
        /// Checks Google's Maven repository for version information
        /// </summary>
        /// <param name="client">An HTTP client to send requests</param>
        /// <param name="groupName">The group name of the dependency</param>
        /// <param name="packageName">The package name of the dependency</param>
        /// <param name="currentVersion">The current version used by the repository</param>
        /// <returns></returns>
        private async Task<string> GetLatestAndroidPackageVersion(HttpClient client, string groupName, string packageName, string currentVersion)
        {
            var cacheKey = $"android:{groupName}:{packageName}";
            if (!_cacheService.TryGetValue(cacheKey, out XmlDocument xmlDocument))
            {
                var groupParts = groupName.Split('.');
                var googleUrl = $"https://dl.google.com/android/maven2/{string.Join('/', groupParts)}/group-index.xml";

                var responseMessage = await client.GetAsync(googleUrl);

                if (responseMessage.IsSuccessStatusCode)
                {
                    await using var stream = await responseMessage.Content.ReadAsStreamAsync();
                    xmlDocument = new XmlDocument();
                    xmlDocument.Load(stream);
                }

                if (xmlDocument != null)
                {
                    _cacheService.Set(cacheKey, xmlDocument);
                }
            }

            if (xmlDocument != null)
            {
                var packageNodes = xmlDocument.GetElementsByTagName(packageName);
                for (int i = 0; i < packageNodes.Count; i++)
                {
                    var node = packageNodes[i];
                    var versionNode = node.Attributes.GetNamedItem("versions");

                    // Versions are listed oldest to newest, reverse so
                    // newest is the first in the array
                    var versions = versionNode.InnerText.Split(',').Reverse().ToArray();

                    var latestVersion = GetLatestVersionBasedOnCurrentVersion(versions, currentVersion);
                    if (!string.IsNullOrEmpty(latestVersion)) return latestVersion;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Parse an array of versions and return the latest acceptable one based on the current version
        /// </summary>
        /// <param name="availableVersions">An array of version strings, newest to oldest</param>
        /// <param name="currentVersion">The current version used</param>
        /// <returns></returns>
        private string GetLatestVersionBasedOnCurrentVersion(string[] availableVersions, string currentVersion)
        {
            if (availableVersions == null || availableVersions.Length <= 0) return string.Empty;

            // If it's supplied, it's in the form of a requirement like "==3.0.2", trim the first 2 characters
            currentVersion = string.IsNullOrEmpty(currentVersion) ? currentVersion : currentVersion.Substring(2);

            SemVersion.TryParse(currentVersion, out SemVersion currentSemVersion);

            // If the repo is currently using a prerelease version of the dependency, it's ok to
            // return a prerelease version as the latest. Otherwise assume that only released
            // versions are acceptable
            bool prereleaseOk = currentSemVersion != null && !string.IsNullOrEmpty(currentSemVersion.Prerelease);

            foreach (var version in availableVersions)
            {
                if (SemVersion.TryParse(version, out SemVersion packageVersion))
                {
                    bool isPrerelease = !string.IsNullOrEmpty(packageVersion.Prerelease);

                    if ((isPrerelease && prereleaseOk) || !isPrerelease)
                    {
                        return version;
                    }
                }
            }

            return string.Empty;
        }
    }
}
