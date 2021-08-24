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
    public class MavenServiceTests : IClassFixture<WebApplicationFactory<SamplesDashboard.Startup>>
    {
        private readonly MavenService _mavenService;
        private readonly ITestOutputHelper _helper;

        public MavenServiceTests(
          WebApplicationFactory<SamplesDashboard.Startup> applicationFactory,
          ITestOutputHelper helper)
        {
            _helper = helper;
            _mavenService = applicationFactory.Services.GetService<MavenService>();
        }

        [Fact]
        public async Task ShouldGetMavenVersions()
        {
            // Arrange
            var packageName = "com.microsoft.graph:microsoft-graph";
            var currentVersion = string.Empty;

            // Act
            var latestVersion = await _mavenService.GetLatestVersion(packageName, currentVersion);

            //Assert
            Assert.False(string.IsNullOrEmpty(latestVersion));
        }

        [Fact]
        public async Task ShouldGetAndroidVersions()
        {
            // Arrange
            var packageName = "androidx.appcompat:appcompat";
            var currentVersion = string.Empty;

            // Act
            var latestVersion = await _mavenService.GetLatestVersion(packageName, currentVersion);

            //Assert
            Assert.False(string.IsNullOrEmpty(latestVersion));
        }
    }
}
