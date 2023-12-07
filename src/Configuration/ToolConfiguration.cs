namespace Xperience.Xman.Configuration
{
    /// <summary>
    /// Represents the configuration of the dotnet tool.
    /// </summary>
    public class ToolConfiguration
    {
        public List<Profile> Profiles { get; set; } = new();
    }
}
