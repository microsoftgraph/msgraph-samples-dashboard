// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using Xunit;
using Xunit.Abstractions;

namespace SamplesDashboardTests
{
    public class NpmServiceTests : IClassFixture<WebApplicationFactory<SamplesDashboard.Startup>>
    {
        private readonly NpmService _npmService;

        public NpmServiceTests(
            WebApplicationFactory<SamplesDashboard.Startup> applicationFactory,
            ITestOutputHelper helper)
        {
            _npmService = applicationFactory.Services.GetService<NpmService>();
        }

        [Fact]
        public async Task ShouldGetNpmVersion()
        {
            // Arrange
            var packageName = "@microsoft/microsoft-graph-client";
            // Act
            var latestVersion = await _npmService.GetLatestVersion(packageName);

            //Assert
            Assert.False(string.IsNullOrEmpty(latestVersion));
        }
    }
}
