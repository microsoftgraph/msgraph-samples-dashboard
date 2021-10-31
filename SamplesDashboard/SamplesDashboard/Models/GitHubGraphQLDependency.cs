// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SamplesDashboard.Models
{
    public class GitHubGraphQLDependency
    {
        public string PackageName { get; set; }
        public string PackageManager { get; set; }
        public string Requirements { get; set; }
        public GitHubGraphQLDependencyRepository Repository { get; set; }
    }
}
