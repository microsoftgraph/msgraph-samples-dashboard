// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace SamplesDashboard.Models
{
    public class GitHubGraphQLRepoData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DateTime UpdatedAt { get; set; }
        public GitHubGraphQLItemCount Issues { get; set; }
        public GitHubGraphQLItemCount PullRequests { get; set; }
        public GitHubGraphQLItemCount Stargazers { get; set; }
        public GitHubGraphQLItemCount Forks { get; set; }
        [JsonPropertyName("defaultBranchRef")]
        public GitHubGraphQLBranch DefaultBranch { get; set; }
        public GitHubGraphQLEdgeCollection<GitHubGraphQLAlert> VulnerabilityAlerts { get; set; }
        [JsonPropertyName("dependencyGraphManifests")]
        public GitHubGraphQLNodeCollection<GitHubGraphQLDependencyManifest> DependencyManifests { get; set; }
    }
}
