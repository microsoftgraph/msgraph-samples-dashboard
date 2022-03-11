// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using GraphQL.Client.Http;
using SamplesDashboard.Models;
using SamplesDashboard.Services;
using SampleDashboardTests.MockHandler;
using Xunit;
using Xunit.Abstractions;

namespace SamplesDashboardTests
{
    public class RepositoriesServiceTests : IClassFixture<WebApplicationFactory<SamplesDashboard.Startup>>
    {
        private readonly RepositoriesService _repositoriesService;
        private readonly ITestOutputHelper _helper;

        private readonly MockGraphQLHttpClientHandler _mockHttpHandler;

        public RepositoriesServiceTests(
          WebApplicationFactory<SamplesDashboard.Startup> applicationFactory,
          ITestOutputHelper helper)
        {
            _mockHttpHandler = new MockGraphQLHttpClientHandler(
                SamplesDashboard.Constants.GitHubGraphQLEndpoint,
                Constants.GraphQLSamplesResponse,
                Constants.GraphQLSdksResponse,
                Constants.GetRepoResponse
            );
            _helper = helper;
            _repositoriesService = applicationFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddHttpClient<GraphQLHttpClient>()
                        .AddHttpMessageHandler(() => {
                            return _mockHttpHandler;
                        });
                });
            }).Services.GetService<RepositoriesService>();
        }

        [Fact]
        public async Task ShouldGetHeaderDetailsFromReadmeAsync()
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
        public async Task ShouldGetHeaderDetailsFromYamlFileAsync()
        {
            //Arrange
            var sampleName = "msgraph-training-java";
            var branchName = "main";

            //Act
            var headerDetails = await _repositoriesService.GetYamlHeader(sampleName, branchName);

            //Assert
            Assert.NotNull(headerDetails);
            _helper.WriteLine(headerDetails.LanguagesString);
            Assert.Equal("java", headerDetails.LanguagesString);
            Assert.Equal("Outlook,Office 365,Microsoft identity platform", headerDetails.ServicesString);
        }

        [Fact]
        public async Task ShouldNotGetHeaderDetailsIfAbsentAsync()
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
        public async Task ShouldGetSamples()
        {
            // Arrange
            var sampleName = "msgraph-training-aspnet-core";

            // Act
            var samples = await _repositoriesService.GetRepositoriesAsync(SamplesDashboard.Constants.Samples);
            var exampleSample = samples.Find((repo) => repo.Name.Equals(sampleName));

            Assert.NotEmpty(samples);
            Assert.IsType<List<Repository>>(samples);
            Assert.NotNull(exampleSample);
        }

        [Fact]
        public async Task ShouldGetSdks()
        {
            // Arrange
            var sdkName = "msgraph-sdk-dotnet";

            // Act
            var sdks = await _repositoriesService.GetRepositoriesAsync(SamplesDashboard.Constants.Sdks);
            var exampleSdk = sdks.Find((repo) => repo.Name.Equals(sdkName));

            Assert.NotEmpty(sdks);
            Assert.IsType<List<Repository>>(sdks);
            Assert.NotNull(exampleSdk);
        }

        [Fact]
        public async Task ShouldGetSampleAndTrainingRepositories()
        {
            //Act
            var samples = await _repositoriesService.GetRepositoriesAsync(SamplesDashboard.Constants.Samples);
            var sampleList = samples.Select(n => n.Name);

            //Assert
            foreach (string sample in sampleList)
            {
                Assert.Contains(SamplesDashboard.Constants.Samples, s => sample.Contains(s, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public async Task ShouldGetSdksRepositories()
        {
            //Act
            var sdks = await _repositoriesService.GetRepositoriesAsync(SamplesDashboard.Constants.Sdks);
            var sdkList = sdks.Select(n => n.Name);

            //Assert
            foreach (var sdk in sdkList)
            {
                Assert.Contains(SamplesDashboard.Constants.Sdks, s => sdk.Contains(s, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public async Task ShouldGetNuGetDependencies()
        {
            //Arrange
            var repository = new Repository
            {
                Name = "msgraph-training-aspnet-core",
                DefaultBranch = "main"
            };

            var data = new GitHubGraphQLRepoData
            {
                DependencyManifests = new GitHubGraphQLNodeCollection<GitHubGraphQLDependencyManifest>
                {
                    Values = new List<GitHubGraphQLDependencyManifest>
                    {
                        new GitHubGraphQLDependencyManifest
                        {
                            FileName = "GraphTutorial.csproj",
                            Dependencies = new GitHubGraphQLNodeCollection<GitHubGraphQLDependency>
                            {
                                Values = new List<GitHubGraphQLDependency>
                                {
                                    new GitHubGraphQLDependency
                                    {
                                        PackageManager = SamplesDashboard.Constants.Nuget,
                                        PackageName = "Microsoft.Graph",
                                        Requirements = "4.0.0"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            //Act
            await _repositoriesService.GetDependencyDataAsync(repository, null, data);
            var packages = repository.Dependencies.Select(d => d.PackageName);

            //Assert
            Assert.IsType<List<Dependency>>(repository.Dependencies);
            Assert.NotEmpty(repository.Dependencies);
            Assert.NotEmpty(packages);
            Assert.Contains("Microsoft.Graph", packages);
        }

        [Fact]
        public async Task ShouldGetNpmDependencies()
        {
            //Arrange
            var repository = new Repository
            {
                Name = "msgraph-training-nodeexpressapp",
                DefaultBranch = "main"
            };

            var data = new GitHubGraphQLRepoData
            {
                DependencyManifests = new GitHubGraphQLNodeCollection<GitHubGraphQLDependencyManifest>
                {
                    Values = new List<GitHubGraphQLDependencyManifest>
                    {
                        new GitHubGraphQLDependencyManifest
                        {
                            FileName = "package.json",
                            Dependencies = new GitHubGraphQLNodeCollection<GitHubGraphQLDependency>
                            {
                                Values = new List<GitHubGraphQLDependency>
                                {
                                    new GitHubGraphQLDependency
                                    {
                                        PackageManager = SamplesDashboard.Constants.Npm,
                                        PackageName = "@microsoft/microsoft-graph-client",
                                        Requirements = "^3.0.0"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            //Act
            await _repositoriesService.GetDependencyDataAsync(repository, null, data);
            var packages = repository.Dependencies.Select(d => d.PackageName);

            //Assert
            Assert.IsType<List<Dependency>>(repository.Dependencies);
            Assert.NotEmpty(repository.Dependencies);
            Assert.NotEmpty(packages);
            Assert.Contains("@microsoft/microsoft-graph-client", packages);
        }

        [Fact]
        public async Task ShouldGetMavenDependencies()
        {
            //Arrange
            var repository = new Repository
            {
                Name = "msgraph-sdk-java",
                DefaultBranch = "main"
            };

            var data = new GitHubGraphQLRepoData
            {
                DependencyManifests = new GitHubGraphQLNodeCollection<GitHubGraphQLDependencyManifest>
                {
                    Values = new List<GitHubGraphQLDependencyManifest>
                    {
                        new GitHubGraphQLDependencyManifest
                        {
                            FileName = "pom.xml",
                            Dependencies = new GitHubGraphQLNodeCollection<GitHubGraphQLDependency>
                            {
                                Values = new List<GitHubGraphQLDependency>
                                {
                                    new GitHubGraphQLDependency
                                    {
                                        PackageManager = SamplesDashboard.Constants.Maven,
                                        PackageName = "com.microsoft.graph:microsoft-graph-core",
                                        Requirements = "3.0.0"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            //Act
            await _repositoriesService.GetDependencyDataAsync(repository, null, data);
            var packages = repository.Dependencies.Select(d => d.PackageName);

            //Assert
            Assert.IsType<List<Dependency>>(repository.Dependencies);
            Assert.NotEmpty(repository.Dependencies);
            Assert.NotEmpty(packages);
            Assert.Contains("com.microsoft.graph:microsoft-graph-core", packages);
        }

        [Fact]
        public async Task ShouldGetGradleDependencies()
        {
            //Arrange
            var repository = new Repository
            {
                Name = "msgraph-training-java",
                DefaultBranch = "main"
            };

            var yamlHeader = new YamlHeader
            {
                DependencyFile = "/demo/graphtutorial/build.gradle"
            };

            var data = new GitHubGraphQLRepoData
            {
                DependencyManifests = new GitHubGraphQLNodeCollection<GitHubGraphQLDependencyManifest>
                {
                    Values = new List<GitHubGraphQLDependencyManifest>()
                }
            };

            //Act
            await _repositoriesService.GetDependencyDataAsync(repository, yamlHeader, data);
            var packages = repository.Dependencies.Select(d => d.PackageName);

            //Assert
            Assert.IsType<List<Dependency>>(repository.Dependencies);
            Assert.NotEmpty(repository.Dependencies);
            Assert.NotEmpty(packages);
            Assert.Contains("com.microsoft.graph:microsoft-graph", packages);
        }

        [Fact]
        public async Task ShouldGetPodfileDependencies()
        {
            //Arrange
            var repository = new Repository
            {
                Name = "msgraph-training-ios-swift",
                DefaultBranch = "main"
            };

            var yamlHeader = new YamlHeader
            {
                DependencyFile = "/demo/GraphTutorial/Podfile"
            };

            var data = new GitHubGraphQLRepoData
            {
                DependencyManifests = new GitHubGraphQLNodeCollection<GitHubGraphQLDependencyManifest>
                {
                    Values = new List<GitHubGraphQLDependencyManifest>()
                }
            };

            //Act
            await _repositoriesService.GetDependencyDataAsync(repository, yamlHeader, data);
            var packages = repository.Dependencies.Select(d => d.PackageName);

            //Assert
            Assert.IsType<List<Dependency>>(repository.Dependencies);
            Assert.NotEmpty(repository.Dependencies);
            Assert.NotEmpty(packages);
            Assert.Contains("msgraphclientsdk", packages);
        }

        [Fact]
        public async Task ShouldGetNullDependencies()
        {
            //Arrange
            var repository = new Repository
            {
                Name = "powershell-intune-samples",
                DefaultBranch = "master"
            };

            var data = new GitHubGraphQLRepoData
            {
                DependencyManifests = new GitHubGraphQLNodeCollection<GitHubGraphQLDependencyManifest>
                {
                    Values = new List<GitHubGraphQLDependencyManifest>()
                }
            };

            //Act
            await _repositoriesService.GetDependencyDataAsync(repository, null, data);

            //Assert
            Assert.IsType<List<Dependency>>(repository.Dependencies);
            Assert.Empty(repository.Dependencies);
        }
    }
}
