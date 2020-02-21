using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

namespace SamplesDashboardTests.Factories
{
    // About WebApplicationFactory
    // https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.2#basic-tests-with-the-default-webapplicationfactory
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1?view=aspnetcore-2.2&viewFallbackFrom=aspnetcore-3.0
    public class BaseWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        public BaseWebApplicationFactory()
        {
            var stringFileData = string.Empty;
            try
            {
                stringFileData = File.ReadAllText("testSettings.json");
            }
            catch (FileNotFoundException ex)
            {
            }

            if (!string.IsNullOrWhiteSpace(stringFileData))
            {
                var settings = JsonConvert.DeserializeObject<JObject>(stringFileData);
                foreach (var setting in settings)
                {
                    var currentValue = setting.Value.ToString();
                    if (!string.IsNullOrWhiteSpace(currentValue))
                    {
                        Environment.SetEnvironmentVariable(setting.Key, setting.Value.ToString());
                    }
                }
            }
        }
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>();

            });
        }
    }
}