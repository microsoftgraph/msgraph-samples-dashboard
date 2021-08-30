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
    public class MicrosoftOpenSourceServiceTests : IClassFixture<BaseWebApplicationFactory<TestStartup>>
    {
        private readonly ITestOutputHelper _helper;
        private readonly MicrosoftOpenSourceService _microsoftService;

        public MicrosoftOpenSourceServiceTests(BaseWebApplicationFactory<TestStartup> applicationFactory, ITestOutputHelper helper)
        {
            _helper = helper;
            _microsoftService = applicationFactory.Services.GetService<MicrosoftOpenSourceService>();
        }

        [Fact]
        public async Task ShouldGetMicrosoftMaintainers()
        {
            // Arrange
            var organization = "microsoftgraph";
            var repoName = "msgraph-training-aspnet-core";

            // Act
            var maintainers = await _microsoftService.GetMicrosoftMaintainers(organization, repoName);

            // Assert
            Assert.Single(maintainers.Maintainers.Individuals);
            Assert.Equal("Jason Johnston (HE/HIM)", maintainers.Maintainers.Individuals[0].DisplayName);
            Assert.NotNull(maintainers.Maintainers.SecurityGroupMail);
            Assert.Equal("graphsampleadmins@microsoft.com", maintainers.Maintainers.SecurityGroupMail);
        }
    }
}
