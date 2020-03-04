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
    public class NugetService
    {
        private readonly SourceRepository _nugetRepository;

        public NugetService()
        {
            _nugetRepository = NugetRepository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        }
        public async Task<string> GetLatestPackageVersion(string packageName)
        {
            using (SourceCacheContext cache = new SourceCacheContext())
            {
                FindPackageByIdResource resource = await _nugetRepository.GetResourceAsync<FindPackageByIdResource>();
                IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(packageName, cache, NullLogger.Instance, CancellationToken.None);

                return versions.LastOrDefault()?.ToString();
            }          
        }
    }
}
