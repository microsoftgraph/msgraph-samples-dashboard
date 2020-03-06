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
        private readonly ITestOutputHelper _helper;

        public NugetServiceTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
        {
            _helper = helper;
            _nugetService = applicationFactory.Services.GetService<NugetService>();
        }

        [Fact]
        public async Task ShouldGetNugetVersions()
        {
            // Arrange
            var packageName = "jQuery";

            // Act
            var latestVersion = await _nugetService.GetLatestPackageVersion(packageName);

            //Assert
            Assert.NotNull(latestVersion);
        }


    }
}
