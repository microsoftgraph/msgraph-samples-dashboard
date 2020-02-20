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
        public Search Search { get; set; }
    }
    public partial class Search
    {
        public List<Node> Nodes { get; set; }
    }
    public partial class Node
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Owner Owner { get; set; }
        public Issues VulnerabilityAlerts { get; set; }
        public Issues Issues { get; set; }
        public Issues PullRequests { get; set; }
        public Issues Stargazers { get; set; }
    }
    public partial class Issues
    {
        public long TotalCount { get; set; }
    }

    public partial class Owner
    {
        public string Login { get; set; }
    }
    public partial class Organization
    {
        public Repository repository { get; set; }
    }

    public class Repository
    {
        public DependencyGraphManifests dependencyGraphManifests { get; set; }
    }

    public partial class DependencyGraphManifests
    {
        public List<DependencyGraphManifestsNode> nodes { get; set; }
    }

    public class DependencyGraphManifestsNode
    {
        public string filename { get; set; }
        public Samples dependencies { get; set; }
    }

    public partial class Samples
    {
        public List<DependenciesNode> nodes { get; set; }
        public long totalCount { get; set; }
    }

    public partial class DependenciesNode
    {
        public string packageManager { get; set; }
        public string packageName { get; set; }
        public string requirements { get; set; }
        public NodeRepository repository { get; set; }
    }

    public partial class NodeRepository
    {
        public string name { get; set; }
        public Releases releases { get; set; }
    }

    public partial class Releases
    {
        public List<ReleasesNode> nodes { get; set; }
    }

    public partial class ReleasesNode
    {
        public string name { get; set; }
        public string tagName { get; set; }
    }

}