// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace SamplesDashboard.Models
{
    public class Repository
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RepoOwner> Owners { get; set; }
        public int SecurityAlerts { get; set; }
        public int Issues { get; set; }
        public int PullRequests { get; set; }
        public int StarGazers { get; set; }
        public int Views { get; set; }
        public int Forks { get; set; }
        public string Url { get; set; }
        public string DefaultBranch { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public string Language { get; set; }
        public string FeatureArea { get; set; }
        public DependencyStatus RepositoryStatus { get; set; }
        public DependencyStatus IdentityStatus { get; set; }
        public DependencyStatus GraphStatus { get; set; }
        public List<Dependency> Dependencies { get; set; }

        public Repository() {}

        public Repository(GitHubGraphQLRepoData data)
        {
            Name = data.Name;
            Description = data.Description;
            Issues = data.Issues.TotalCount;
            PullRequests = data.PullRequests.TotalCount;
            StarGazers = data.Stargazers.TotalCount;
            Forks = data.Forks.TotalCount;
            Url = data.Url;
            DefaultBranch = data.DefaultBranch?.Name;
            LastUpdated = data.UpdatedAt;
        }
    }
}
