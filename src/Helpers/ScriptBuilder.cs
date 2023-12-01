using Xperience.Xman.Options;

namespace Xperience.Xman.Helpers
{
    /// <summary>
    /// Contains methods for generating scripts to execute with <see cref="CommandHelper"/>.
    /// </summary>
    public class ScriptBuilder
    {
        private string currentScript;
        private readonly ScriptType currentScriptType;
        private const string INSTALL_PROJECT_SCRIPT = $"dotnet new {nameof(InstallOptions.Template)} -n {nameof(InstallOptions.ProjectName)}";
        private const string INSTALL_DATABASE_SCRIPT = $"dotnet kentico-xperience-dbmanager -- -s \"{nameof(InstallOptions.ServerName)}\" -d \"{nameof(InstallOptions.DatabaseName)}\" -a \"{nameof(InstallOptions.AdminPassword)}\"";
        private const string UNINSTALL_TEMPLATE_SCRIPT = "dotnet new uninstall kentico.xperience.templates";
        private const string INSTALL_TEMPLATE_SCRIPT = "dotnet new install kentico.xperience.templates";
        private const string UPDATE_PACKAGE_SCRIPT = $"dotnet add package {nameof(UpdateOptions.PackageName)}";
        private const string UPDATE_DATABASE_SCRIPT = $"dotnet run --no-build --kxp-update";
        private const string BUILD_SCRIPT = $"dotnet build";
        

        /// <summary>
        /// Initializes a new instance of <see cref="ScriptBuilder"/>.
        /// </summary>
        /// <param name="type">The type of script to generate.</param>
        public ScriptBuilder(ScriptType type)
        {
            currentScriptType = type;
            currentScript = type switch
            {
                ScriptType.ProjectInstall => INSTALL_PROJECT_SCRIPT,
                ScriptType.DatabaseInstall => INSTALL_DATABASE_SCRIPT,
                ScriptType.TemplateUninstall => UNINSTALL_TEMPLATE_SCRIPT,
                ScriptType.TemplateInstall => INSTALL_TEMPLATE_SCRIPT,
                ScriptType.PackageUpdate => UPDATE_PACKAGE_SCRIPT,
                ScriptType.DatabaseUpdate => UPDATE_DATABASE_SCRIPT,
                ScriptType.BuildProject => BUILD_SCRIPT,
                _ => String.Empty,
            };
        }


        /// <summary>
        /// Gets the generated script.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public string Build()
        {
            if (!ValidateScript())
            {
                throw new InvalidOperationException("The script is empty or contains placeholder values.");
            }

            return currentScript;
        }


        /// <summary>
        /// Appends a version number to the script if <paramref name="version"/> is not null.
        /// </summary>
        public ScriptBuilder AppendVersion(Version? version)
        {
            if (currentScriptType.Equals(ScriptType.TemplateInstall) && version is not null)
            {
                currentScript += $"::{version}";
            }
            else if (currentScriptType.Equals(ScriptType.PackageUpdate) && version is not null)
            {
                currentScript += $" --version {version}";
            }

            return this;
        }


        /// <summary>
        /// Appends " --cloud" to the script if <paramref name="useCloud"/> is true.
        /// </summary>
        public ScriptBuilder AppendCloud(bool useCloud)
        {
            if (currentScriptType.Equals(ScriptType.ProjectInstall) && useCloud)
            {
                currentScript += " --cloud";
            }

            return this;
        }


        /// <summary>
        /// Replaces script placeholders with the provided option values. If a property is <c>null</c> or emtpy,
        /// the placeholder remains in the script.
        /// </summary>
        public ScriptBuilder WithOptions(IWizardOptions options)
        {
            // Replace all placeholders in script with option values if non-null or empty
            foreach (var prop in options.GetType().GetProperties())
            {
                var value = prop.GetValue(options)?.ToString() ?? String.Empty;
                if (!String.IsNullOrEmpty(value))
                {
                    currentScript = currentScript.Replace(prop.Name, value);
                }
            }

            return this;
        }


        private bool ValidateScript()
        {
            var propertyNames = typeof(InstallOptions).GetProperties().Select(p => p.Name);

            return !String.IsNullOrEmpty(currentScript) && !propertyNames.Any(currentScript.Contains);
        }
    }


    public enum ScriptType
    {
        /// <summary>
        /// The script which installs new Xperience by Kentico project files.
        /// </summary>
        ProjectInstall,


        /// <summary>
        /// The script which installs a new Xperience by Kentico database.
        /// </summary>
        DatabaseInstall,


        /// <summary>
        /// The script which uninstalls the Xperience by Kentico templates.
        /// </summary>
        TemplateUninstall,


        /// <summary>
        /// The script which installs the Xperience by Kentico templates.
        /// </summary>
        TemplateInstall,


        /// <summary>
        /// The script which updates the Xperience by Kentico packages.
        /// </summary>
        PackageUpdate,


        /// <summary>
        /// The script which updates the Xperience by Kentico database.
        /// </summary>
        DatabaseUpdate,


        /// <summary>
        /// The script which builds the project.
        /// </summary>
        BuildProject
    }
}
