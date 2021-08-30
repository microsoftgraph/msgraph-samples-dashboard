// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace SamplesDashboard.Models
{
    public enum PackageStatus
    {
        Unknown = 0,
        UpToDate = 1,
        PatchUpdate = 2,
        MinorVersionUpdate = 3,
        MajorVersionUpdate = 4,
        UrgentUpdate = 5
    }
}
