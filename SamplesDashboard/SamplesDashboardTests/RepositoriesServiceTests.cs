// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using System.Threading.Tasks;
using SamplesDashboardTests.Factories;
using Xunit;
using Xunit.Abstractions;
using SamplesDashboard.Models;
using System.Collections.Generic;
using SamplesDashboard;
using System.Linq;
using System;

namespace SamplesDashboardTests
{
    public class RepositoriesServiceTests : IClassFixture<BaseWebApplicationFactory<TestStartup>>
    {
        private readonly ITestOutputHelper _helper;
        private readonly RepositoriesService _repositoriesService;

        public RepositoriesServiceTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
        {
            _helper = helper;
            _repositoriesService = applicationFactory.Services.GetService<RepositoriesService>();
        }

        [Fact]
        public async Task ShouldGetHeaderDetailsAsync()
        {
            //Arrange
            var sampleName = "aspnetcore-connect-sample";

            //Act
            var headerDetails = await _repositoriesService.GetHeaderDetails(sampleName);
            _helper.WriteLine(headerDetails["languages"]);

            //Assert
            Assert.NotNull(headerDetails);
            Assert.True(headerDetails["languages"] == "aspx,csharp");
            Assert.True(headerDetails["services"] == "Microsoft identity platform");
        }

        [Fact]
        public async Task ShouldGetHeaderDetailsAsync2()
        {
            //Arrange
            var sampleName = "uwp-csharp-excel-snippets-rest-sample";

            //Act
            var headerDetails = await _repositoriesService.GetHeaderDetails(sampleName);

            //Assert
            Assert.NotNull(headerDetails);
            Assert.True(headerDetails["languages"] == "csharp,uwp");
            Assert.True(headerDetails["services"] == "Excel");
        }

        [Fact]
        public async Task ShouldGetSamples()
        {
            // Arrange
            var sampleName = "msgraph-training-aspnetmvcapp";
            string name = " sample OR training";

            // Act
            var samples = await _repositoriesService.GetRepositories(name);
            var exampleSample = samples.Find((node) => node.Name.Equals(sampleName));

            Assert.NotEmpty(samples);
            Assert.IsType<List<Node>>(samples);
            Assert.NotNull(exampleSample);
            Assert.Equal("microsoftgraph", exampleSample.Owner.Login);
        }

        [Fact]
        public async Task ShouldGetSdksList()
        {
            // Arrange
            var sdkName = "msgraph-sdk-dotnet";
            string name = " sdk";

            // Act
            var sdks = await _repositoriesService.GetRepositories(name);
            var exampleSdk = sdks.Find((node) => node.Name.Equals(sdkName));

            Assert.NotEmpty(sdks);
            Assert.IsType<List<Node>>(sdks);
            Assert.NotNull(exampleSdk);
            Assert.Equal("microsoftgraph", exampleSdk.Owner.Login);
        }

        [Fact]
        public async Task ShouldGetSampleAndTrainingRepositories()
        {
            //Arrange
            string name = " sample OR training";

            //Act
            var samples = await _repositoriesService.GetRepositories(name);
            var sampleList = samples.Select(n => n.Name);

            //Assert
            foreach (string sample in sampleList)
            {
                Assert.True(sample.Contains("sample", StringComparison.OrdinalIgnoreCase) || sample.Contains("training", StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public async Task ShouldGetSdksRepositories()
        {
            //Arrange
            string name = " sdk";

            //Act
            var sdks = await _repositoriesService.GetRepositories(name);
            var sdkList = sdks.Select(n => n.Name);

            //Assert
            foreach (string sdk in sdkList)
            {
                Assert.True(sdk.Contains("sdk", StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public async Task ShouldGetDependencies()
        {
            //Arrange
            var name = "msgraph-training-aspnetmvcapp";
            var packageManager = "NUGET";

            //Act
            var repository = await _repositoriesService.GetRepository(name);
            var res = repository.DependencyGraphManifests.Nodes.SelectMany(n => n.Dependencies.Nodes.Select(n => n.packageManager)).ToList();
            var library = repository.DependencyGraphManifests.Nodes.SelectMany(n => n.Dependencies.Nodes.Select(n => n.packageName)).Contains("Microsoft.Graph");

            //Assert
            Assert.NotNull(repository);
            Assert.True(library);
            Assert.Equal(packageManager, res.FirstOrDefault());
            Assert.IsType<Repository>(repository);

        }

        [Fact]
        public async Task ShouldGetNullDependencies()
        {
            //Arrange
            var name = "powershell-intune-samples";

            //Act
            var repository = await _repositoriesService.GetRepository(name);
            var nodes = repository.DependencyGraphManifests.Nodes.Select(n => n.Dependencies.Nodes);

            //Assert
            Assert.NotNull(repository);
            Assert.Empty(nodes);
        }       

        [Theory]
        [InlineData("1.2.3", "1.2.3", PackageStatus.UpToDate)]
        [InlineData("1.2.3.4", "1.2.3.4", PackageStatus.UpToDate)]
        [InlineData("1.2.1", "1.3.1", PackageStatus.Update)]
        [InlineData("2.2.1", "3.0.0", PackageStatus.Update)]
        [InlineData("1.2.0", "1.2.1", PackageStatus.UrgentUpdate)]
        [InlineData("1.2.0", "v1.2.1", PackageStatus.UrgentUpdate)]
        [InlineData("v1.2.0", "v1.2.1", PackageStatus.UrgentUpdate)]
        [InlineData("1.2.0", "2.1.0-Preview.1", PackageStatus.Update)]
        [InlineData("1.2.0", "2.1.0-alpha.1", PackageStatus.Update)]
        [InlineData("2.1.0", "2.1.0-Preview.1", PackageStatus.UrgentUpdate)]
        [InlineData("2.1.0", "2.1.0-2", PackageStatus.UrgentUpdate)]
        [InlineData("2.1.0", "3.1.0-2", PackageStatus.Update)]
        [InlineData("2.1.0", "1.8<2.1", PackageStatus.Unknown)]
        [InlineData("2.1.0", "Unknown", PackageStatus.Unknown)]
        [InlineData("2.1.0", "", PackageStatus.Unknown)]
        [InlineData("2.1.0", null, PackageStatus.Unknown)]
        public void ShouldDetermineStatusFromVersion(string sampleVersion, string latestVersion, PackageStatus expectedStatus)
        {
            // Act
            var repositoryStatus = _repositoriesService.CalculateStatus(sampleVersion, latestVersion);

            // Assert
            Assert.Equal(expectedStatus, repositoryStatus);
        }

        [Fact]
        public async Task ShouldSetCurrentVersionOnEmptyStrings()
        {
            //Arrange
            var sample = "msgraph-training-phpapp";

            //Act 
            var dependency = await _repositoriesService.GetRepository(sample);
            var latestVersion = _repositoriesService.UpdateRepositoryStatus(dependency).ToString();

            Assert.NotNull(latestVersion);
        }
    }
}
 