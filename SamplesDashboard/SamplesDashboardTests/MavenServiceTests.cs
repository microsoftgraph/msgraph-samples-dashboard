// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using System.Threading.Tasks;
using SamplesDashboardTests.Factories;
using Xunit;
using Xunit.Abstractions;

namespace SamplesDashboardTests
{
    public class MavenServiceTests : IClassFixture<BaseWebApplicationFactory<TestStartup>>
    {
        private readonly MavenService _mavenService;
        private readonly ITestOutputHelper _helper;

        public MavenServiceTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
        {
            _helper = helper;
            _mavenService = applicationFactory.Services.GetService<MavenService>();
        }

        [Fact]
        public async Task ShouldGetMavenVersions()
        {
            // Arrange
            var packageName = "com.microsoft.graph:microsoft-graph";

            // Act
            var latestVersion = await _mavenService.GetLatestVersion(packageName);

            //Assert
            Assert.False(string.IsNullOrEmpty(latestVersion));
        }
    }
}
