using Xperience.Xman.Commands;

namespace Xperience.Xman.Options
{
    /// <summary>
    /// The options used to create a new Continuous Development profile, used by <see cref="ContinuousDevelopmentCommand"/>.
    /// </summary>
    public class NewCDOptions : IWizardOptions
    {
        /// <summary>
        /// The root folder which contains individual CD folders and configuration files.
        /// </summary>
        public string? RootConfigPath { get; set; }


        /// <summary>
        /// The name of the environment (e.g. "DEV," "PROD").
        /// </summary>
        public string? EnvironmentName { get; set; }
    }
}
