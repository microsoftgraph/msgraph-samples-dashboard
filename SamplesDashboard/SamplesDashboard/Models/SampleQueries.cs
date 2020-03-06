// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Newtonsoft.Json;
using SamplesDashboard.Models;
using System;
using System.Collections.Generic;

namespace SamplesDashboard
{
    public partial class Welcome
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
    }
    public partial class Node
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("vulnerabilityAlerts")]
        public Issues VulnerabilityAlerts { get; set; }

        [JsonProperty("issues")]
        public Issues Issues { get; set; }

        [JsonProperty("pullRequests")]
        public Issues PullRequests { get; set; }

        [JsonProperty("stargazers")]
        public Issues Stargazers { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("forkCount")]
        public long ForkCount { get; set; }

        public string Language { get; internal set; }

        public string FeatureArea { get; internal set; }
    }
    public partial class Issues
    {
        [JsonProperty("totalCount")]
        public long TotalCount { get; set; }
    }

    public partial class Owner
    {
        [JsonProperty("login")]
        public string Login { get; set; }
    }
    public class Organization
    {
        [JsonProperty("repository")]
        public Repository Repository { get; set; }
    }

    public class Repository
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("dependencyGraphManifests")]
        public DependencyGraphManifests DependencyGraphManifests { get; set; }
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
}