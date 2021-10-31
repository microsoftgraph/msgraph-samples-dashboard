// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace SamplesDashboard.Models
{
    public class GitHubGraphQLDependencyManifest
    {
        public string FileName { get; set; }
        public GitHubGraphQLNodeCollection<GitHubGraphQLDependency> Dependencies { get; set; }

        public static GitHubGraphQLDependencyManifest InitializeForFile(string fileName)
        {
            var manifest = new GitHubGraphQLDependencyManifest();
            manifest.FileName = fileName;
            manifest.Dependencies = new GitHubGraphQLNodeCollection<GitHubGraphQLDependency>();
            manifest.Dependencies.Values = new List<GitHubGraphQLDependency>();

            return manifest;
        }
    }
}
