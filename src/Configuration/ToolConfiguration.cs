using Xperience.Xman.Options;

namespace Xperience.Xman.Configuration
{
    /// <summary>
    /// Represents the configuration of the dotnet tool.
    /// </summary>
    public class ToolConfiguration
    {
        /// <summary>
        /// The version of the dotnet tool that last managed this configuration.
        /// </summary>
        public Version? Version { get; set; }


        /// <summary>
        /// The registered profiles found in the configuration file.
        /// </summary>
        public List<Profile> Profiles { get; set; } = new();


        /// <summary>
        /// The currently active profile name as stored in the configuration file.
        /// </summary>
        public string? CurrentProfile { get; set; }


        /// <summary>
        /// The <see cref="InstallOptions"/> stored in the configuration file.
        /// </summary>
        public InstallOptions? DefaultInstallOptions { get; set; }
    }
}
