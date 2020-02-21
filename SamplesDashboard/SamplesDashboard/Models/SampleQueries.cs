// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Newtonsoft.Json;
using System.Collections.Generic;

namespace SamplesDashboard
{
    public class Data
    {
        public Organization organization { get; set; }
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
        public Repository repository { get; set; }
    }

    public class Repository
    {
        public DependencyGraphManifests dependencyGraphManifests { get; set; }
    }

    public class DependencyGraphManifests
    {
        public List<DependencyGraphManifestsNode> nodes { get; set; }
    }

    public class DependencyGraphManifestsNode
    {
        public string filename { get; set; }
        public Samples dependencies { get; set; }
    }

    public class Samples
    {
        public List<DependenciesNode> nodes { get; set; }
        public long totalCount { get; set; }
    }

    public class DependenciesNode
    {
        public string packageManager { get; set; }
        public string packageName { get; set; }
        public string requirements { get; set; }
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