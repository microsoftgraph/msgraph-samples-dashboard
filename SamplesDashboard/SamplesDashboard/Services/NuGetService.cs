// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NugetRepository = NuGet.Protocol.Core.Types.Repository;
using NuGet.Versioning;

namespace SamplesDashboard.Services
{
    public class NuGetService
    {
        private readonly SourceRepository _nuGetRepository;
        private readonly double _cacheLifetime;
        private readonly IMemoryCache _cache;

        public NuGetService(
            IConfiguration configuration,
            IMemoryCache memoryCache)
        {
            _cacheLifetime = configuration.GetValue<double>(Constants.CacheLifetime);
            _cache = memoryCache;
            _nuGetRepository = NugetRepository.Factory
                .GetCoreV3("https://api.nuget.org/v3/index.json");
        }

        public async Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageName)
        {
            using var cache = new SourceCacheContext();
            var resource = await _nuGetRepository.GetResourceAsync<FindPackageByIdResource>();
            var versions = await resource.GetAllVersionsAsync(
                packageName, cache, NullLogger.Instance, CancellationToken.None);

            return versions;
        }

        public async Task<string> GetLatestVersion(string packageName, string currentVersion)
        {
            NuGetVersion latestVersion = null;
            NuGetVersion latestPreview = null;

            var cacheKey = $"nuget:{packageName}";
            if (!_cache.TryGetValue(cacheKey, out string latestAndPreview))
            {
                var versions = await GetPackageVersions(packageName);

                var nonPreviewVersions = (from version in versions
                    where !version.IsPrerelease
                    select version).ToList();

                var previewVersions = (from version in versions
                    where version.IsPrerelease
                    select version).ToList();

                latestVersion = nonPreviewVersions.LastOrDefault();
                latestPreview = previewVersions.LastOrDefault();

                var latestVersionString = latestVersion?.ToString() ?? string.Empty;
                var latestPreviewString = latestPreview?.ToString() ?? string.Empty;

                // Save the versions into the cache
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheLifetime));
                _cache.Set(cacheKey, $"{latestVersionString};{latestPreviewString}");
            }
            else
            {
                var versions = latestAndPreview.Split(';');
                latestVersion = string.IsNullOrEmpty(versions[0]) ? null : new NuGetVersion(versions[0]);
                latestPreview = versions.Length <= 1 || string.IsNullOrEmpty(versions[1]) ? null : new NuGetVersion(versions[1]);
            }

            try
            {
                var currentVer = new NuGetVersion(currentVersion);

                if (currentVer.IsPrerelease && latestVersion != null && latestVersion < currentVer)
                {
                    return latestPreview?.ToString() ?? string.Empty;
                }
            }
            catch (ArgumentException)
            {
                // GitHub can return requirement strings that NuGet doesn't understand
                if (currentVersion.Contains("preview", StringComparison.OrdinalIgnoreCase))
                {
                    return latestPreview?.ToString() ?? string.Empty;
                }
            }

            return latestVersion?.ToString() ?? latestPreview?.ToString() ?? string.Empty;
        }
    }
}
