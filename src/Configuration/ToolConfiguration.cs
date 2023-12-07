using Xperience.Xman.Options;

namespace Xperience.Xman.Configuration
{
    /// <summary>
    /// Represents the configuration of the dotnet tool.
    /// </summary>
    public class ToolConfiguration
    {
        public List<Profile> Profiles { get; set; } = new();


        public InstallOptions? DefaultInstallOptions { get; set; }
    }
}
