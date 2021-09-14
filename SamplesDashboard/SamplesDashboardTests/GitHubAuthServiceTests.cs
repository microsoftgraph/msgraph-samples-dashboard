// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using Xunit;
using Xunit.Abstractions;

namespace SampleDashboardTests
{
    public class GitHubAuthServiceTests : IClassFixture<WebApplicationFactory<SamplesDashboard.Startup>>
    {
        private GitHubAuthService _gitHubAuthService;
        private ITestOutputHelper _helper;

        public GitHubAuthServiceTests(
            WebApplicationFactory<SamplesDashboard.Startup> factory,
            ITestOutputHelper helper
        )
        {
            _helper = helper;
            _gitHubAuthService = factory.Services.GetService<GitHubAuthService>();
        }

        [Fact]
        public async Task ShouldGetGitHubToken()
        {
            // Arrange

            // Act
            var token = await _gitHubAuthService.GetGitHubAppToken();

            // Assert
            Assert.NotNull(token);
            Assert.False(string.IsNullOrEmpty(token), "Token was empty");
            _helper.WriteLine(token);
        }
    }
}
