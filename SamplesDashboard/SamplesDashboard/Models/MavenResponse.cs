// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace SamplesDashboard.Models
{
    public class MavenResponse
    {
        public int NumFound { get; set; }
        public List<MavenPackageInfo> Docs { get; set; }
    }
}
