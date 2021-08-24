// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SamplesDashboard.Models
{
    public class GitHubGraphQLSearchResult
    {
        [JsonPropertyName("nodes")]
        public List<GitHubGraphQLRepoData> Results { get; set; }
        public GitHubGraphQLPageInfo PageInfo { get; set; }
    }
}
