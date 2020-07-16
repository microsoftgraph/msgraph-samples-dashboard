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
    public class NugetServiceTests : IClassFixture<BaseWebApplicationFactory<TestStartup>>
    {
        private readonly NugetService _nugetService;

        public NugetServiceTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
        {
            _nugetService = applicationFactory.Services.GetService<NugetService>();            
        }

        [Fact]
        public async Task ShouldGetNugetVersions()
        {
            // Arrange
            var packageName = "jQuery";
            // Act
            var nugetVersions = await _nugetService.GetPackageVersions(packageName);

            //Assert
            Assert.NotNull(nugetVersions);
        }
       
    }
}
