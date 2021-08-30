// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Semver;

namespace SamplesDashboard.Models
{
    public class Dependency
    {
        public string PackageName { get; set; }
        public string ManifestFile { get; set; }
        public string CurrentVersion { get; set; }
        public string LatestVersion { get; set; }
        public DependencyStatus Status { get; private set; }

        public void CalculateStatus(List<GitHubGraphQLAlert> alerts)
        {
            // Calculate status
            if (alerts != null && alerts.Any(
                v => v.Info?.SecurityVulnerability?.Package?.Name?
                    .Equals(PackageName, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                Status = DependencyStatus.UrgentUpdate;
            }
            else
            {
                Status = CalculateStatus(CurrentVersion, LatestVersion);
            }
        }

        internal DependencyStatus CalculateStatus(string currentVersion, string latestVersion)
        {
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(latestVersion))
            {
                return DependencyStatus.Unknown;
            }

            if (latestVersion.StartsWith("v"))
            {
                latestVersion = latestVersion.Substring(1);
            }

            if (currentVersion.Equals(latestVersion, StringComparison.OrdinalIgnoreCase))
            {
                return DependencyStatus.UpToDate;
            }

            if (!SemVersion.TryParse(currentVersion.Split(',').First().Trim(), out SemVersion current) ||
                !SemVersion.TryParse(latestVersion.Trim(), out SemVersion latest))
            {
                return DependencyStatus.Unknown;
            }

            var status = current.CompareTo(latest);
            if (status == 0)
            {
                return DependencyStatus.UpToDate;
            }

            if (current.Major == latest.Major && current.Minor == latest.Minor)
            {
                return DependencyStatus.PatchUpdate;
            }

            if (current.Major == latest.Major)
            {
                return DependencyStatus.MinorVersionUpdate;
            }

            if (status < 0)
            {
                return DependencyStatus.MajorVersionUpdate;
            }

            return DependencyStatus.Unknown;
        }
    }
}
