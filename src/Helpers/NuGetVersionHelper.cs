using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Xperience.Xman.Helpers
{
    public static class NuGetVersionHelper
    {
        /// <summary>
        /// Gets all versions of the provided NuGet package.
        /// </summary>
        public static async Task<IEnumerable<NuGetVersion>> GetPackageVersions(string package)
        {
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            return await resource.GetAllVersionsAsync(
                package,
                new SourceCacheContext(),
                NullLogger.Instance,
                CancellationToken.None);
        }
    }
}