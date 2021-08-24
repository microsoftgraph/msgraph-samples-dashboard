// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace SamplesDashboard.Models
{
    public class NpmQueryResult
    {
        [JsonPropertyName("dist-tags")]
        public NpmTag Tags { get; set; }
    }
}
