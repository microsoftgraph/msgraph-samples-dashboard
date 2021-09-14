// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SamplesDashboard.Models
{
    public class GitHubGraphQLAlert
    {
        [JsonPropertyName("node")]
        public GitHubGraphQLAlertInfo Info { get; set; }
    }
}
