// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using Xunit;
using Xunit.Abstractions;

namespace SamplesDashboardTests
{
    public class ManifestFromFileServiceTests : IClassFixture<WebApplicationFactory<SamplesDashboard.Startup>>
    {
        private readonly ManifestFromFileService _manifestFromFileService;
        private readonly ITestOutputHelper _helper;

        public ManifestFromFileServiceTests(
          WebApplicationFactory<SamplesDashboard.Startup> applicationFactory,
          ITestOutputHelper helper)
        {
            _helper = helper;
            _manifestFromFileService = applicationFactory.Services.GetService<ManifestFromFileService>();
        }

        [Fact]
        public async Task ShouldGetDependenciesFromGradleFile()
        {
            // Arrange
            var repoName = "msgraph-training-android";
            var defaultBranch = "main";
            var dependencyFile = "/demo/GraphTutorial/app/build.gradle";

            // Act
            var manifest = await _manifestFromFileService
              .BuildDependencyManifestFromFile(repoName, defaultBranch, dependencyFile);

            //Assert
            Assert.NotNull(manifest);
            Assert.NotEmpty(manifest.Dependencies.Values);
            Assert.Equal(SamplesDashboard.Constants.Gradle, manifest.Dependencies.Values[0].PackageManager);
        }

        [Fact]
        public async Task ShouldGetDependenciesFromPodfile()
        {
            // Arrange
            var repoName = "msgraph-training-ios-swift";
            var defaultBranch = "main";
            var dependencyFile = "/demo/GraphTutorial/Podfile";

            // Act
            var manifest = await _manifestFromFileService
              .BuildDependencyManifestFromFile(repoName, defaultBranch, dependencyFile);

            //Assert
            Assert.NotNull(manifest);
            Assert.NotEmpty(manifest.Dependencies.Values);
            Assert.Equal(SamplesDashboard.Constants.CocoaPods, manifest.Dependencies.Values[0].PackageManager);
        }
    }
}
