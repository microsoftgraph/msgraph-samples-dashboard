// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using SamplesDashboardTests.Factories;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SamplesDashboardTests
{
    public class NpmServiceTests : IClassFixture<BaseWebApplicationFactory<TestStartup>>
    {
        private readonly NpmService _npmService;
        private readonly ITestOutputHelper _helper;

        public NpmServiceTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
        {
            _helper = helper;
            _npmService = applicationFactory.Services.GetService<NpmService>();
        }

        [Fact]
        public async Task ShouldGetNpmVersions()
        {
            // Arrange
            var packageName = "@hapi/boom";

            // Act
            var latestVersion = await _npmService.GetLatestVersion(packageName);

            //Assert
            Assert.NotNull(latestVersion);

        }

    }
}
