// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SamplesDashboard.DataFiles;
using SamplesDashboard.Tasks.Definitions;

namespace SamplesDashboard.Tasks
{
    /// <summary>
    ///     Startup task that runs before the request pipeline is setup to migrate the db.
    ///     Technique described here https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-2/
    /// </summary>
    public class ApplicationDbMigratorStartupTask: IDbMigratorSeederStartupTask
    {
        private readonly ILogger<ApplicationDbMigratorStartupTask> _logger;
        private readonly IServiceProvider _provider;

        public ApplicationDbMigratorStartupTask(IServiceProvider provider,
           ILogger<ApplicationDbMigratorStartupTask> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public string Name => nameof(ApplicationDbMigratorStartupTask);

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await MigrateDatabase(cancellationToken);          
        }
        /// <summary>
        /// Run any pending migrations
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task MigrateDatabase(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting Pending Migrations...");
            using (var scope = _provider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
                var migrations = pendingMigrations.ToList();
                if (!migrations.Any())
                {
                    _logger.LogInformation("No Migrations Found!!!");
                }
                else
                {
                    _logger.LogInformation($"Found {migrations.Count} Migrations...Executing!!!");
                    await dbContext.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Migrations Complete!");
                }
            }
        }
    }
}
