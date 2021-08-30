// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace SamplesDashboard.Models
{
    public class MicrosoftMaintainerStatus
    {
        [JsonPropertyName("repo")]
        public string Repository { get; set; }
        [JsonPropertyName("org")]
        public string Organization { get; set; }
        public MicrosoftMaintainers Maintainers { get; set; }
    }
}
