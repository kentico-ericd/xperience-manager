using Xperience.Xman.Options;

namespace Xperience.Xman.Configuration
{
    /// <summary>
    /// Represents the configuration of the dotnet tool.
    /// </summary>
    public class ToolConfiguration
    {
        /// <summary>
        /// The registered profiles found in the configuration file.
        /// </summary>
        public List<Profile> Profiles { get; set; } = new();


        /// <summary>
        /// The currently active profile as stored in the configuration file.
        /// </summary>
        public Profile? CurrentProfile { get; set; }


        /// <summary>
        /// The <see cref="InstallOptions"/> stored in the configuration file.
        /// </summary>
        public InstallOptions? DefaultInstallOptions { get; set; }
    }
}
