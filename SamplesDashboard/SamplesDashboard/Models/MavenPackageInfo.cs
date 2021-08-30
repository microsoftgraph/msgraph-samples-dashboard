// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace SamplesDashboard.Models
{
    public class MavenPackageInfo
    {
        public string Id { get; set; }

        [JsonPropertyName("v")]
        public string Version { get; set; }
    }
}
