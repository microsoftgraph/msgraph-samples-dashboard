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
    public class MicrosoftOpenSourceServiceTests : IClassFixture<WebApplicationFactory<SamplesDashboard.Startup>>
    {
        private readonly MicrosoftOpenSourceService _microsoftOpenSourceService;
        private readonly ITestOutputHelper _helper;

        public MicrosoftOpenSourceServiceTests(
          WebApplicationFactory<SamplesDashboard.Startup> applicationFactory,
          ITestOutputHelper helper)
        {
            _helper = helper;
            _microsoftOpenSourceService = applicationFactory.Services.GetService<MicrosoftOpenSourceService>();
        }

        [Fact]
        public async Task ShouldGetMicrosoftMaintainers()
        {
            // Arrange
            var organization = "microsoftgraph";
            var repoName = "msgraph-training-aspnet-core";

            // Act
            var maintainers = await _microsoftOpenSourceService.GetMicrosoftMaintainers(organization, repoName);

            //Assert
            Assert.Single(maintainers.Maintainers.Individuals);
            Assert.Equal("Jason Johnston", maintainers.Maintainers.Individuals[0].DisplayName);
            Assert.NotNull(maintainers.Maintainers.SecurityGroupMail);
            Assert.Equal("graphsampleadmins@microsoft.com", maintainers.Maintainers.SecurityGroupMail);
        }
    }
}
