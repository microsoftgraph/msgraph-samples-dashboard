// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SamplesDashboard.Models
{
    public class GitHubGraphQLNodeCollection<T>
    {
        [JsonPropertyName("nodes")]
        public List<T> Values { get; set; }
    }
}
