// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SamplesDashboard.Models
{
    public class GitHubGraphQLEdgeCollection<T>
    {
        public int TotalCount { get; set; }
        [JsonPropertyName("edges")]
        public List<T> Values { get; set; }
    }
}
