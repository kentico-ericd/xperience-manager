using Xperience.Xman.Configuration;
using Xperience.Xman.Options;

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
        public Task AddProfile(Profile profile);


        /// <summary>
        /// Gets the <see cref="InstallOptions"/> specified by the tool configuration file, or a new instance if
        /// the configuration can't be read.
        /// </summary>
        public Task<InstallOptions> GetDefaultInstallOptions();
    }
}
