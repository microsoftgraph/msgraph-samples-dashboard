// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using SamplesDashboard.Extensions;

namespace SamplesDashboard
{
    public static class Constants
    {
        private static readonly IEnumerable<string> SdkList = new[]
        {
            "sdk", "microsoft-graph-explorer", "devx-api", "raptor", "msgraph-cli", "samples-dashboard"
        };
        private static readonly IEnumerable<string> SamplesList = new[] {"sample", "training"};
        public static readonly string Sdks = SdkList.BuildQueryString();
        public static readonly string Samples = SamplesList.BuildQueryString();
        public const string Timeout = "timeout";
    }
}
