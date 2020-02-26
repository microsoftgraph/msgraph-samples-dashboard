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
    public class SampleServiceTests : IClassFixture<BaseWebApplicationFactory<TestStartup>>
    {
        private readonly ITestOutputHelper _helper;
        private readonly SampleService _sampleService;

        public SampleServiceTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
        {
            _helper = helper;
            _sampleService = applicationFactory.Services.GetService<SampleService>();

        }

        [Fact]
        public async Task ShouldGetHeaderDetailsAsync()
        {
            //Arrange
            var sampleName = "powershell-intune-samples";

            //Act
            var headerDetails = await _sampleService.GetHeaderDetails(sampleName);

            //Assert
            Assert.NotNull(headerDetails);
            Assert.True(headerDetails["languages"] == "powershell");
            Assert.True(headerDetails["services"] == "Intune");
            _helper.WriteLine(string.Join("\n", headerDetails));

        }

        [Fact]
        public async Task ShouldGetHeaderDetailsAsync2()
        {
            //Arrange
            var sampleName = "uwp-csharp-excel-snippets-rest-sample";

            //Act
            var headerDetails = await _sampleService.GetHeaderDetails(sampleName);

            //Assert
            Assert.NotNull(headerDetails);
            Assert.True(headerDetails["languages"] == "csharp,uwp");
            Assert.True(headerDetails["services"] == "Excel");
            _helper.WriteLine(string.Join("\n", headerDetails));

        }

        [Fact]
        public async Task ShouldGetHeaderDetailsAsync3()
        {
            //Arrange
            var sampleName = "ios-swift-faceapi-sample";

            //Act
            var headerDetails = await _sampleService.GetHeaderDetails(sampleName);

            //Assert
            Assert.NotNull(headerDetails);
            Assert.True(headerDetails["languages"] == "swift");
            Assert.True(headerDetails["services"] == "Office 365,Users");
            _helper.WriteLine(string.Join("\n", headerDetails));

        }
    }
}
 