using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using SamplesDashboardTests.Factories;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SamplesDashboardTests
{
    public class AzureSdkServiceTests : IClassFixture<BaseWebApplicationFactory<TestStartup>>
    {
        private readonly AzureSdkService _azureService;
        private readonly ITestOutputHelper _helper;

        public AzureSdkServiceTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
        {
            _helper = helper;
            _azureService = applicationFactory.Services.GetService<AzureSdkService>();
        }

        [Fact]
        public async Task ShouldGetAzureSdkDictionary()
        {
            // Act
            var azureVersions = await _azureService.FetchAzureSdkVersions();

            //Assert
            Assert.NotNull(azureVersions);
            Assert.NotEmpty(azureVersions);
        }

        [Fact]
        public async Task ShouldGetAzureSdkVersions()
        {
            //Arrange
            var library = "Moq";

            // Act
            var azureVersions = await _azureService.GetAzureSdkVersions(library);

            //Assert
            Assert.NotEmpty(azureVersions);
        }
    }
}
