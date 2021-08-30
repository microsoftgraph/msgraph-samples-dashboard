// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using SamplesDashboard.Tasks.Definitions;
using System.Threading;
using System.Threading.Tasks;

namespace SamplesDashboard.Tasks.Definitions
{
    public interface IDbMigratorSeederStartupTask : IStartupTask
    {       
        Task MigrateDatabase(CancellationToken cancellationToken = default);
    }
}