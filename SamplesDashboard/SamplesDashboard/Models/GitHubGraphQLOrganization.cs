// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SamplesDashboard.Models
{
    public class GitHubGraphQLOrganization
    {
        public GitHubGraphQLRepoData Repository { get; set; }
    }
}
