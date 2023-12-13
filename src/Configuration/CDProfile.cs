namespace Xperience.Xman.Configuration
{
    /// <summary>
    /// Represents a Continuous Development configuration.
    /// </summary>
    public class CDProfile
    {
        /// <summary>
        /// The name of the Continuous Development environment.
        /// </summary>
        public string? EnvironmentName { get; set; }


        /// <summary>
        /// The absolute path to the repository configuration file.
        /// </summary>
        public string? ConfigPath { get; set; }
    }
}
