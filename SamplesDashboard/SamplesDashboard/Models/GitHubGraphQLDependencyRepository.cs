// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SamplesDashboard.Models
{
    public class GitHubGraphQLDependencyRepository
    {
        public string Name { get; set; }
        public GitHubGraphQLNodeCollection<GitHubGraphQLDependencyRelease> Releases { get; set; }
    }
}
