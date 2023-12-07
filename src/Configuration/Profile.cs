namespace Xperience.Xman.Configuration
{
    /// <summary>
    /// Represents the configuration of an individual Xperience by Kentico installation.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// The Xperience by Kentico project name.
        /// </summary>
        public string? ProjectName { get; set; }


        /// <summary>
        /// The absolute path to the installation's root folder.
        /// </summary>
        public string? WorkingDirectory { get; set; }
    }
}
