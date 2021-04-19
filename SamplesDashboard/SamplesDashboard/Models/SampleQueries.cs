// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Newtonsoft.Json;
using SamplesDashboard.Models;
using System;
using System.Collections.Generic;

namespace SamplesDashboard
{
    public partial class SampleData
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
    public class Data
    {
        [JsonProperty("organization")]
        public Organization Organization { get; set; }
        [JsonProperty("search")]
        public Search Search { get; set; }
    }
    public partial class Search
    {
        [JsonProperty("nodes")]
        public List<Node> Nodes { get; set; }

        [JsonProperty("pageInfo")]
        public PageInfo PageInfo { get; set; }
    }
    public partial class Node
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public Dictionary<string, string> OwnerProfiles { get; set; }

        [JsonProperty("vulnerabilityAlerts")]
        public VulnerabilityAlerts VulnerabilityAlerts { get; set; }

        [JsonProperty("issues")]
        public Issues Issues { get; set; }

        [JsonProperty("pullRequests")]
        public Issues PullRequests { get; set; }

        [JsonProperty("stargazers")]
        public Issues Stargazers { get; set; }

        public int? Views { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("contributors")]
        public Contributors Contributors { get; set; }

        [JsonProperty("forks")]
        public Forks Forks { get; set; }

        [JsonProperty("defaultBranchRef")]
        public Branch DefaultBranch { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset LastUpdated { get; set; }

        public string Language { get; internal set; }

        public string FeatureArea { get; internal set; }

        public bool HasDependencies { get; set; }

        public PackageStatus RepositoryStatus { get; internal set; }

        public PackageStatus IdentityStatus { get; set; }

        public PackageStatus GraphStatus { get; set; }
    }
    public partial class VulnerabilityAlerts
    {
        [JsonProperty("totalCount")]
        public long TotalCount { get; set; }

        [JsonProperty("edges")]
        public Edge[] Edges { get; set; }
    }

    public partial class Edge
    {
        [JsonProperty("node")]
        public EdgeNode Node { get; set; }
    }

    public partial class EdgeNode
    {
        [JsonProperty("securityVulnerability")]
        public SecurityVulnerability SecurityVulnerability { get; set; }
    }

    public partial class SecurityVulnerability
    {
        [JsonProperty("package")]
        public Package Package { get; set; }
    }

    public partial class Package
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public partial class Forks
    {
        [JsonProperty("totalCount")]
        public long TotalCount { get; set; }
    }

    public partial class Issues
    {
        [JsonProperty("totalCount")]
        public long TotalCount { get; set; }
    }

    public partial class ViewData
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public partial class PageInfo
    {
        [JsonProperty("endCursor")]
        public string EndCursor { get; set; }

        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }
    }

    public partial class Contributors
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("url")]
        public Uri HtmlUrl { get; set; }
    }

    public class Organization
    {
        [JsonProperty("repository")]
        public Repository Repository { get; set; }
    }

    public class Repository
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("vulnerabilityAlerts")]
        public VulnerabilityAlerts VulnerabilityAlerts { get; set; }

        [JsonProperty("dependencyGraphManifests")]
        public DependencyGraphManifests DependencyGraphManifests { get; set; }

        [JsonProperty("defaultBranchRef")]
        public Branch DefaultBranch { get; set; }

        public PackageStatus highestStatus { get; set; }

        public PackageStatus IdentityStatus { get; set; }

        public PackageStatus GraphStatus { get; set; }
    }

    public class DependencyGraphManifests
    {
        [JsonProperty("nodes")]
        public DependencyGraphManifestsNode[] Nodes { get; set; }
    }

    public class DependencyGraphManifestsNode
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("dependencies")]
        public Dependencies Dependencies { get; set; }
    }
    public partial class Dependencies
    {
        [JsonProperty("nodes")]
        public DependenciesNode[] Nodes { get; set; }
    }
    public class Samples
    {
        public List<DependenciesNode> nodes { get; set; }
        public long totalCount { get; set; }
    }

    public class DependenciesNode
    {
        [JsonProperty("status")]
        public PackageStatus status { get; set; }
        public string packageManager { get; set; }
        public string packageName { get; set; }
        public string requirements { get; set; }
        public string latestVersion { get; set; }
        public Packages repository { get; set; }
        public string azureSdkVersion { get; set; }
    }

    public class Packages
    {
        public string name { get; set; }

        public Releases releases { get; set; }

    }

    public class Releases
    {
        public List<ReleasesNode> nodes { get; set; }
    }

    public class ReleasesNode
    {
        public string name { get; set; }
        public string tagName { get; set; }
    }

    public class Branch
    {
        public string Name { get; set; }
    }

    public enum SupportedDependencyFileType
    {
        Unsupported, Gradle, PodFile
    }
}