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
    public class CocoaPodsServiceTests : IClassFixture<BaseWebApplicationFactory<TestStartup>>
    {
        private readonly CocoaPodsService _cocoaPodsService;
        private readonly ITestOutputHelper _helper;

        public CocoaPodsServiceTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
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
            Assert.False(string.IsNullOrEmpty(latestVersion));
        }
    }
}
