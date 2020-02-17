// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using SamplesDashboard.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SamplesDashboardTests
{
    public class SampleServiceTests
    {
        [Fact]
        public async Task ShouldGetSampleLanguagesAsync()
        {
            //Arrange
            var sampleName = "powershell-intune-samples";

            //Act
            var languages = await SampleService.GetLanguages(sampleName);

            //Assert
            Assert.NotNull(languages);
            Assert.Equal("powershell", languages.First());
        }

        [Fact]
        public async Task ShouldGetSampleFeaturesAsync()
        {
            //Arrange
            var sampleName = "powershell-intune-samples";

            //Act
            var services = await SampleService.GetFeatures(sampleName);

            //Assert
            Assert.NotNull(services);
            Assert.Equal("Intune", services.First());
        }

        [Fact]
        public async Task ShouldGetNullSampleLanguageAsync()
        {
            //Arrange
            var sampleName = "msgraph-training-aspnetmvcapp";

            //Act
            var languages = await SampleService.GetLanguages(sampleName);

            //Assert
            Assert.Empty(languages);
        }

        [Fact]
        public async Task ShouldGetNullSampleFeaturesAsync()
        {
            //Arrange
            var sampleName = "msgraph-training-aspnetmvcapp";

            //Act
            var services = await SampleService.GetFeatures(sampleName);

            //Assert
            Assert.Empty(services);
        }
    }
}
