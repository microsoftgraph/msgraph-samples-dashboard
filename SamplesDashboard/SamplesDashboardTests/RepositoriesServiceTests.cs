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
            var branchName = "main";

            //Act
            var headerDetails = await _repositoriesService.GetYamlHeader(sampleName, branchName);

            //Assert
            Assert.NotNull(headerDetails);
            _helper.WriteLine(headerDetails.LanguagesString);
            Assert.Equal("aspx,csharp", headerDetails.LanguagesString);
            Assert.Equal("Microsoft identity platform", headerDetails.ServicesString);
        }

        [Fact]
        public async Task ShouldGetHeaderDetailsAsync2()
        {
            //Arrange
            var sampleName = "meetings-capture-sample";
            var branchName = "master";

            //Act
            var headerDetails = await _repositoriesService.GetYamlHeader(sampleName, branchName);

            //Assert
            Assert.Null(headerDetails);
        }

        [Fact]
        public async Task ShouldGetHeaderDetailsAsync3()
        {
            //Arrange
            var sampleName = "nodejs-webhooks-rest-sample";
            var branchName = "main";

            //Act
            var headerDetails = await _repositoriesService.GetYamlHeader(sampleName, branchName);

            //Assert
            Assert.NotNull(headerDetails);
            _helper.WriteLine(headerDetails.LanguagesString);
            Assert.Equal("nodejs,javascript", headerDetails.LanguagesString);
            Assert.Equal("Outlook,Office 365,Microsoft identity platform", headerDetails.ServicesString);
        }

        [Fact]
        public async Task ShouldGetHeaderDetailsAsync4()
        {
            //Arrange
            var sampleName = "python-security-rest-sample";
            var branchName = "master";

            //Act
            var headerDetails = await _repositoriesService.GetYamlHeader(sampleName, branchName);

            //Assert
            Assert.NotNull(headerDetails);
            Assert.Equal("python,html", headerDetails.LanguagesString);
            Assert.Equal("Security", headerDetails.ServicesString);
        }

        [Fact]
        public async Task ShouldGetSamples()
        {
            // Arrange
            var sampleName = "msgraph-training-aspnetmvcapp";

            // Act
            var samples = await _repositoriesService.GetRepositories(Constants.Samples);
            var exampleSample = samples.Find((node) => node.Name.Equals(sampleName));

            Assert.NotEmpty(samples);
            Assert.IsType<List<Node>>(samples);
            Assert.NotNull(exampleSample);
        }

        [Fact]
        public async Task ShouldGetSdksList()
        {
            // Arrange
            var sdkName = "msgraph-sdk-dotnet";

            // Act
            var sdks = await _repositoriesService.GetRepositories(Constants.Sdks);
            var exampleSdk = sdks.Find((node) => node.Name.Equals(sdkName));

            Assert.NotEmpty(sdks);
            Assert.IsType<List<Node>>(sdks);
            Assert.NotNull(exampleSdk);
        }

        [Fact]
        public async Task ShouldGetSampleAndTrainingRepositories()
        {
            //Act
            var samples = await _repositoriesService.GetRepositories(Constants.Samples);
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
            //Act
            var sdks = await _repositoriesService.GetRepositories(Constants.Sdks);
            var sdkList = sdks.Select(n => n.Name);

            //Assert
            foreach (var sdk in sdkList)
            {
                Assert.Contains(Constants.Sdks, s => sdk.Contains(s, StringComparison.OrdinalIgnoreCase));
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
        public async Task ShouldGetMavenDependencies()
        {
            //Arrange
            var name = "msgraph-sdk-java-core";
            var packageManager = "MAVEN";

            //Act
            var repository = await _repositoriesService.GetRepository(name);
            var res = repository.DependencyGraphManifests.Nodes.SelectMany(n => n.Dependencies.Nodes.Select(n => n.packageManager)).ToList();
            var versions = repository.DependencyGraphManifests.Nodes.SelectMany(n => n.Dependencies.Nodes.Select(n => n.latestVersion)).ToList();

            //Assert
            Assert.NotNull(repository);
            Assert.Equal(packageManager, res.FirstOrDefault());
            Assert.IsType<Repository>(repository);
            Assert.DoesNotContain(versions, v => string.IsNullOrEmpty(v));
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
        [InlineData("1.2.1", "1.3.1", PackageStatus.MinorVersionUpdate)]
        [InlineData("2.2.1", "3.0.0", PackageStatus.MajorVersionUpdate)]
        [InlineData("1.2.0", "1.2.1", PackageStatus.PatchUpdate)]
        [InlineData("1.2.0", "v1.2.1", PackageStatus.PatchUpdate)]
        [InlineData("v1.2.0", "v1.2.1", PackageStatus.PatchUpdate)]
        [InlineData("1.2.0", "2.1.0-Preview.1", PackageStatus.MajorVersionUpdate)]
        [InlineData("1.2.0", "2.1.0-alpha.1", PackageStatus.MajorVersionUpdate)]
        [InlineData("2.1.0", "2.1.0-Preview.1", PackageStatus.PatchUpdate)]
        [InlineData("2.1.0", "2.1.0-2", PackageStatus.PatchUpdate)]
        [InlineData("2.1.0", "3.1.0-2", PackageStatus.MajorVersionUpdate)]
        [InlineData("2.1.0", "1.8<2.1", PackageStatus.Unknown)]
        [InlineData("2.1.0", "Unknown", PackageStatus.Unknown)]
        [InlineData("2.1.0", "", PackageStatus.Unknown)]
        [InlineData("2.1.0", null, PackageStatus.Unknown)]
        [InlineData("2.0,< 3.0", "2.0.2", PackageStatus.PatchUpdate)]
        public void ShouldDetermineStatusFromVersion(string sampleVersion, string latestVersion, PackageStatus expectedStatus)
        {
            //Act
            var repositoryStatus = _repositoriesService.CalculateStatus(sampleVersion, latestVersion);

            //Assert
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

        [Fact]
        public async Task ShouldGetDependenciesFromGradleFile()
        {
            // Arrange
            var repoName = "msgraph-training-android";
            var defaultBranch = "main";
            var dependencyFile = "/demo/GraphTutorial/app/build.gradle";

            // Act
            var dependencies = await _repositoriesService.BuildDependencyGraphFromFile(repoName, defaultBranch, dependencyFile);

            Assert.NotNull(dependencies);
            Assert.NotEmpty(dependencies);
        }

        [Fact]
        public async Task ShouldGetDependenciesFromPodfile()
        {
            // Arrange
            var repoName = "msgraph-training-ios-swift";
            var defaultBranch = "main";
            var dependencyFile = "/demo/GraphTutorial/Podfile";

            // Act
            var dependencies = await _repositoriesService.BuildDependencyGraphFromFile(repoName, defaultBranch, dependencyFile);

            Assert.NotNull(dependencies);
            Assert.NotEmpty(dependencies);
        }
    }
}
