// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace SamplesDashboard.Tasks.Definitions
{
    /// <summary>
    /// Implement and register with DI to create new startup tasks that run before Request pipeline is setup.
    /// Such as Database migration or one-off tasks required by the system.
    /// https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-2/
    /// </summary>
    public interface IStartupTask
    {
        string Name { get; }
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}