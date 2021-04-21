// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace SamplesDashboard.Models
{
    public class MicrosoftMaintainerStatus
    {
        [JsonProperty("repo")]
        public string Repository { get; set; }
        [JsonProperty("org")]
        public string Organization { get; set; }
        public MicrosoftMaintainers Maintainers { get; set; }
    }
}
