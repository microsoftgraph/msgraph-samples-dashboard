// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace SamplesDashboard.Models
{
    public class NpmQuery
    {
        [JsonProperty("dist-tags")]
        public DistTags DistTags { get; set; }
        
    }
    public partial class DistTags
    {
        [JsonProperty("latest")]
        public string Latest { get; set; }
    }
}
