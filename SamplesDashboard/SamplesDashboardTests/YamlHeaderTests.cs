// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Models;
using System.Net.Http;
using System.Threading.Tasks;
using SamplesDashboardTests.Factories;
using SamplesDashboardTests.MessageHandlers;
using Xunit;
using Xunit.Abstractions;

namespace SamplesDashboardTests
{
    public class YamlHeaderTests : IClassFixture<BaseWebApplicationFactory<TestStartup>>
    {
        private HttpClient _httpClient;

        public YamlHeaderTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
        {
            var factory = applicationFactory.Services.GetRequiredService<IHttpClientFactory>();
            _httpClient = factory.CreateClient();
        }

        [Fact]
        public void ShouldParseYamlContent()
        {
            // Arrange
            var yamlContent =
@"
languages:
- java
extensions:
    services:
    - Outlook
    - OneDrive
    - Teams
dependencyFile: demo/GraphTutorial/app/build.gradle
noDependencies: false";

            // Act
            var header = YamlHeader.FromString(yamlContent);

            // Assert
            Assert.NotNull(header);
            Assert.Equal("demo/GraphTutorial/app/build.gradle", header.DependencyFile);
            Assert.NotNull(header.Languages);
            Assert.Single(header.Languages);
            Assert.Contains("java", header.Languages);
            Assert.NotNull(header.Extensions);
            Assert.NotNull(header.Extensions.Services);
            Assert.Equal(3, header.Extensions.Services.Length);
            Assert.Contains("Outlook", header.Extensions.Services);
            Assert.False(header.NoDependencies);
            Assert.True(header.NoDependenciesWasSpecified);
        }

        [Fact]
        public void ShouldMergeButNotOverwrite()
        {
            // Arrange
            var mergeHeaderContent =
@"
languages:
- csharp
extensions:
    services:
    - Outlook
    - OneDrive
    - Teams
noDependencies: true";

            var mergeIntoHeaderContent =
@"
languages:
- java
dependencyFile: demo/GraphTutorial/app/build.gradle
noDependencies: false";

            // Act
            var mergeHeader = YamlHeader.FromString(mergeHeaderContent);
            var mergeIntoHeader = YamlHeader.FromString(mergeIntoHeaderContent);
            mergeIntoHeader.MergeWith(mergeHeader);

            // Assert
            Assert.Single(mergeIntoHeader.Languages);
            Assert.Contains("java", mergeIntoHeader.Languages);
            Assert.DoesNotContain("csharp", mergeIntoHeader.Languages);
            Assert.NotNull(mergeIntoHeader.Extensions);
            Assert.NotNull(mergeIntoHeader.Extensions.Services);
            Assert.Equal(3, mergeIntoHeader.Extensions.Services.Length);
            Assert.Contains("Outlook", mergeIntoHeader.Extensions.Services);
            Assert.False(mergeIntoHeader.NoDependencies);
        }

        [Fact]
        public async Task ShouldGetHeaderFromRepo()
        {
            // Arrange
            var repoName = "msgraph-sdk-java";
            var branch = "dev";

            // Act
            var header = await YamlHeader.GetFromRepo(_httpClient, repoName, branch);

            // Assert
            Assert.NotNull(header);
        }

        [Fact]
        public async Task ShouldCombineMultipleSources()
        {
            // Arrange
            var readmeHeader =
@"---
languages:
- java
extensions:
    services:
    - Outlook
    - OneDrive
    - Teams
---

# README TITLE

Some text";

            var repoNameHeader =
@"
extensions:
    services:
    - Outlook
    - OneDrive
dependencyFile: demo/GraphTutorial/app/build.gradle";

            var devxHeader =
@"
dependencyFile: demo/GraphTutorial/app/dependencies.gradle";

            var contentHandler = new StringContentHandler(new string[] { readmeHeader, repoNameHeader, devxHeader });
            var httpClient = new HttpClient(contentHandler);

            // Act
            var header = await YamlHeader.GetFromRepo(httpClient, "repo", "branch");

            // Assert
            Assert.Single(header.Languages);
            Assert.Contains("java", header.Languages);
            Assert.NotNull(header.Extensions);
            Assert.NotNull(header.Extensions.Services);
            Assert.Equal(2, header.Extensions.Services.Length);
            Assert.Contains("Outlook", header.Extensions.Services);
            Assert.DoesNotContain("Teams", header.Extensions.Services);
            Assert.NotNull(header.DependencyFile);
            Assert.Equal("demo/GraphTutorial/app/dependencies.gradle", header.DependencyFile);
            Assert.False(header.NoDependencies);
        }
    }
}
