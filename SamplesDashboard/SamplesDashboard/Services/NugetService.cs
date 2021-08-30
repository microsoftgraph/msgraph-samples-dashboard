// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NugetRepository = NuGet.Protocol.Core.Types.Repository;
using System.Threading;
using NuGet.Common;
using NuGet.Versioning;

namespace SamplesDashboard.Services
{
    /// <summary>
    /// This class contains methods for querying npm to get the latest package release versions
    /// </summary>
    public class NugetService
    {
        private readonly SourceRepository _nugetRepository;

        public NugetService()
        {
            _nugetRepository = NugetRepository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        }

        /// <summary>
        /// Accesses Nuget's registry via index.json file(entry point) and fetches latest package release versions
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns>Latest version</returns>
        public async Task<IEnumerable<NuGetVersion>> GetPackageVersions(string packageName)
        {
            using (SourceCacheContext cache = new SourceCacheContext())
            {
                FindPackageByIdResource resource = await _nugetRepository.GetResourceAsync<FindPackageByIdResource>();
                IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(packageName, cache, NullLogger.Instance, CancellationToken.None);

                return versions;
            }          
        }
    }
}
