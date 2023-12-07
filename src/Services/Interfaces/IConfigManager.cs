using Xperience.Xman.Options;
using Xperience.Xman.Configuration;

namespace Xperience.Xman.Services
{
    /// <summary>
    /// Contains methods for managing <see cref="ToolConfiguration"/>.
    /// </summary>
    public interface IConfigManager : IService
    {
        /// <summary>
        /// Ensures that the configuration file is present in the executing directory.
        /// </summary>
        public Task EnsureConfigFile();


        /// <summary>
        /// Adds a profile to the <see cref="ToolConfiguration.Profiles"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task AddProfile(InstallOptions options);
    }
}
