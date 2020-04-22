using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SamplesDashboard.Tasks.Definitions;

namespace SamplesDashboard.Extensions
{
    public static class StartupTaskWebHostExtensions
    {
        public static async Task RunWithTasksAsync(this IWebHost webHost, CancellationToken cancellationToken = default)
        {
            // Load all tasks from DI
            var logger = webHost.Services.GetService<ILogger<IHost>>();
            var startupTasks = webHost.Services.GetServices<IStartupTask>();
            var startupTasksList = startupTasks.ToList();
            logger.LogInformation($"Found {startupTasksList.Count} Startup Task(s)");
            if (startupTasksList.Any())
            {
                // Execute all the tasks
                foreach (var startupTask in startupTasksList)
                {
                    logger.LogInformation($"Executing {startupTask.Name}...");
                    // Execute all the tasks
                    await startupTask.ExecuteAsync(cancellationToken);
                    logger.LogInformation($"Executing {startupTask.Name} Completed!!!");
                }
            }
            // Start the tasks as normal
            await webHost.RunAsync(cancellationToken);
        }
    }
}