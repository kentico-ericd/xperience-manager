using Xperience.Xman.Options;

namespace Xperience.Xman.Services
{
    /// <summary>
    /// Contains methods for generating scripts to execute with <see cref="IShellRunner"/>.
    /// </summary>
    public interface IScriptBuilder : IService
    {
        /// <summary>
        /// Appends " --cloud" to the script if <paramref name="useCloud"/> is true.
        /// </summary>
        public IScriptBuilder AppendCloud(bool useCloud);


        /// <summary>
        /// Appends a version number to the script if <paramref name="version"/> is not null.
        /// </summary>
        public IScriptBuilder AppendVersion(Version? version);


        /// <summary>
        /// Gets the generated script.
        /// </summary>
        public string Build();


        /// <summary>
        /// Initializes a new instance of <see cref="ScriptBuilder"/>.
        /// </summary>
        /// <param name="type">The type of script to generate.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public IScriptBuilder SetScript(ScriptType type);


        /// <summary>
        /// Replaces script placeholders with the provided option values. If a property is <c>null</c> or emtpy,
        /// the placeholder remains in the script.
        /// </summary>
        public IScriptBuilder WithOptions(IWizardOptions options);
    }
}