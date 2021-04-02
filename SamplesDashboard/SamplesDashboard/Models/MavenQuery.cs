// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace SamplesDashboard.Models
{
    public class MavenQuery
    {
        public MavenResponse Response { get; set; }

    }
    public partial class MavenResponse
    {
        public int NumFound { get; set; }
        public List<MavenPackageInfo> Docs { get; set; }
    }

    public partial class MavenPackageInfo
    {
        public string Id { get; set; }
        public string LatestVersion { get; set; }
    }
}
