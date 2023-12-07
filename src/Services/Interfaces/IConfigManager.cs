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
        /// Gets the current profile, or <c>null</c> if not set. If there is only one profile registered, that
        /// profile is automatically selected.
        /// </summary>
        public Task<Profile?> GetCurrentProfile();


        /// <summary>
        /// Gets the <see cref="ToolConfiguration"/> represented by the physical file.
        /// </summary>
        public Task<ToolConfiguration> GetConfig();


        /// <summary>
        /// Gets the <see cref="InstallOptions"/> specified by the tool configuration file, or a new instance if
        /// the configuration can't be read.
        /// </summary>
        public Task<InstallOptions> GetDefaultInstallOptions();


        /// <summary>
        /// Sets the currently active profile.
        /// </summary>
        /// <returns><c>True</c> if a profile was loaded and set.</returns>
        public Task<bool> TrySetCurrentProfile(string profileName);
    }
}
