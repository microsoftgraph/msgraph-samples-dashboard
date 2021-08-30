// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SamplesDashboard.Models
{
    public enum DependencyStatus
    {
        Unknown = 0,
        UpToDate = 1,
        PatchUpdate = 2,
        MinorVersionUpdate = 3,
        MajorVersionUpdate = 4,
        UrgentUpdate = 5
    }
}
