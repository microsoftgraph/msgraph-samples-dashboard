// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using System.Linq;
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

        //[Fact]
        //public async Task ShouldGetSampleLanguagesAsync()
        //{
        //    //Arrange
        //    var sampleName = "powershell-intune-samples";

        //    //Act

        //    var languages = await _sampleService.GetLanguages(sampleName);

        //    //Assert
        //    Assert.NotNull(languages);
        //    Assert.Equal("powershell", languages.First());
        //    _helper.WriteLine(string.Join("\n", languages));
        //}

        [Fact]
        public async Task ShouldGetSampleFeaturesAsync()
        {
            //Arrange
            var sampleName = "powershell-intune-samples";

            //Act
            var services = await _sampleService.GetFeatures(sampleName);

            //Assert
            Assert.NotNull(services);
            Assert.Equal("Intune", services.First());
        }

        //[Fact]
        //public async Task ShouldGetNullSampleLanguageAsync()
        //{
        //    //Arrange
        //    var sampleName = "msgraph-training-aspnetmvcapp";

        //    //Act
        //    var languages = await _sampleService.GetLanguages(sampleName);

        //    //Assert
        //    Assert.Empty(languages);
        //}

        [Fact]
        public async Task ShouldGetNullSampleFeaturesAsync()
        {
            //Arrange
            var sampleName = "msgraph-training-aspnetmvcapp";

            //Act
            var services = await _sampleService.GetFeatures(sampleName);

            //Assert
            Assert.Empty(services);
        }
    }
}
