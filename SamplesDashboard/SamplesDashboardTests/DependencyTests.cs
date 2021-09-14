// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using SamplesDashboard.Models;
using SamplesDashboard.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace SamplesDashboardTests
{
    public class DependencyTests
    {
        private readonly ITestOutputHelper _helper;

        public DependencyTests(
          ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Theory]
        [InlineData("= 1.2.3", "1.2.3", DependencyStatus.UpToDate)]
        [InlineData("= 1.2.3.4", "1.2.3.4", DependencyStatus.UpToDate)]
        [InlineData("= 1.2.1", "1.3.1", DependencyStatus.MinorVersionUpdate)]
        [InlineData("= 2.2.1", "3.0.0", DependencyStatus.MajorVersionUpdate)]
        [InlineData("= 1.2.0", "1.2.1", DependencyStatus.PatchUpdate)]
        [InlineData("= 1.2.0", "v1.2.1", DependencyStatus.PatchUpdate)]
        [InlineData("= v1.2.0", "v1.2.1", DependencyStatus.PatchUpdate)]
        [InlineData("= 1.2.0", "2.1.0-Preview.1", DependencyStatus.MajorVersionUpdate)]
        [InlineData("= 1.2.0", "2.1.0-alpha.1", DependencyStatus.MajorVersionUpdate)]
        [InlineData("= 2.1.0", "2.1.0-Preview.1", DependencyStatus.PatchUpdate)]
        [InlineData("= 2.1.0", "2.1.0-2", DependencyStatus.PatchUpdate)]
        [InlineData("= 2.1.0", "3.1.0-2", DependencyStatus.MajorVersionUpdate)]
        [InlineData("= 2.1.0", "1.8<2.1", DependencyStatus.Unknown)]
        [InlineData("= 2.1.0", "Unknown", DependencyStatus.Unknown)]
        [InlineData("= 2.1.0", "", DependencyStatus.Unknown)]
        [InlineData("= 2.1.0", null, DependencyStatus.Unknown)]
        [InlineData("= 2.0,< 3.0", "2.0.2", DependencyStatus.PatchUpdate)]
        public void ShouldDetermineStatusFromVersion(string sampleVersion, string latestVersion, DependencyStatus expectedStatus)
        {
            //Act
            var dependency = new Dependency();
            var repositoryStatus = dependency.CalculateStatus(sampleVersion.NormalizeRequirementsString(), latestVersion);

            //Assert
            Assert.Equal(expectedStatus, repositoryStatus);
        }
    }
}
