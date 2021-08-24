// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using Xunit;
using Xunit.Abstractions;

namespace SampleDashboardTests
{
    public class NuGetServiceTests : IClassFixture<WebApplicationFactory<SamplesDashboard.Startup>>
    {
        private readonly NuGetService _nugetService;

        public NuGetServiceTests(
            WebApplicationFactory<SamplesDashboard.Startup> applicationFactory,
            ITestOutputHelper helper)
        {
            _nugetService = applicationFactory.Services.GetService<NuGetService>();
        }

        [Fact]
        public async Task ShouldGetNuGetVersion()
        {
            // Arrange
            var packageName = "jQuery";
            var currentVersion = "1.0.0";
            // Act
            var latestVersion = await _nugetService.GetLatestVersion(packageName, currentVersion);

            //Assert
            Assert.False(string.IsNullOrEmpty(latestVersion));
        }
    }
}
