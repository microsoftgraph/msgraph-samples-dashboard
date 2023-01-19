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
    public class CocoaPodsServiceTests : IClassFixture<WebApplicationFactory<SamplesDashboard.Startup>>
    {
        private readonly CocoaPodsService _cocoaPodsService;
        private readonly ITestOutputHelper _helper;

        public CocoaPodsServiceTests(
          WebApplicationFactory<SamplesDashboard.Startup> applicationFactory,
          ITestOutputHelper helper)
        {
            _helper = helper;
            _cocoaPodsService = applicationFactory.Services.GetService<CocoaPodsService>();
        }

        [Fact]
        public async Task ShouldGetCocoaPodsVersions()
        {
            // Arrange
            var packageName = "MSGraphClientSDK";

            // Act
            var latestVersion = await _cocoaPodsService.GetLatestVersion(packageName);

            //Assert
            Assert.NotNull(latestVersion);
        }
    }
}
