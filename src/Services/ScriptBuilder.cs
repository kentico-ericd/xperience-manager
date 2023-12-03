using Xperience.Xman.Options;

namespace Xperience.Xman.Services
{
    public class ScriptBuilder : IScriptBuilder
    {
        private string currentScript = String.Empty;
        private ScriptType currentScriptType;
        private const string BUILD_SCRIPT = $"dotnet build";
        private const string INSTALL_PROJECT_SCRIPT = $"dotnet new {nameof(InstallOptions.Template)} -n {nameof(InstallOptions.ProjectName)}";
        private const string INSTALL_DATABASE_SCRIPT = $"dotnet kentico-xperience-dbmanager -- -s \"{nameof(InstallOptions.ServerName)}\" -d \"{nameof(InstallOptions.DatabaseName)}\" -a \"{nameof(InstallOptions.AdminPassword)}\"";
        private const string UNINSTALL_TEMPLATE_SCRIPT = "dotnet new uninstall kentico.xperience.templates";
        private const string INSTALL_TEMPLATE_SCRIPT = "dotnet new install kentico.xperience.templates";
        private const string UPDATE_PACKAGE_SCRIPT = $"dotnet add package {nameof(UpdateOptions.PackageName)}";
        private const string UPDATE_DATABASE_SCRIPT = $"dotnet run --no-build --kxp-update";
        private const string CI_STORE_SCRIPT = $"dotnet run --no-build --kxp-ci-store";
        private const string CI_RESTORE_SCRIPT = $"dotnet run --no-build --kxp-ci-restore";


        public string Build()
        {
            if (!ValidateScript())
            {
                throw new InvalidOperationException("The script is empty or contains placeholder values.");
            }

            return currentScript;
        }


        public IScriptBuilder AppendVersion(Version? version)
        {
            if (currentScriptType.Equals(ScriptType.TemplateInstall))
            {
                currentScript += $"::{version}";
            }
            else if (currentScriptType.Equals(ScriptType.PackageUpdate) && version is not null)
            {
                currentScript += $" --version {version}";
            }

            return this;
        }


        public IScriptBuilder AppendCloud(bool useCloud)
        {
            if (currentScriptType.Equals(ScriptType.ProjectInstall) && useCloud)
            {
                currentScript += " --cloud";
            }

            return this;
        }


        public IScriptBuilder WithOptions(IWizardOptions options)
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


        public IScriptBuilder SetScript(ScriptType type)
        {
            if (type.Equals(ScriptType.None))
            {
                throw new InvalidOperationException("Invalid script type.");
            }

            currentScriptType = type;
            currentScript = type switch
            {
                ScriptType.BuildProject => BUILD_SCRIPT,
                ScriptType.ProjectInstall => INSTALL_PROJECT_SCRIPT,
                ScriptType.DatabaseInstall => INSTALL_DATABASE_SCRIPT,
                ScriptType.TemplateUninstall => UNINSTALL_TEMPLATE_SCRIPT,
                ScriptType.TemplateInstall => INSTALL_TEMPLATE_SCRIPT,
                ScriptType.PackageUpdate => UPDATE_PACKAGE_SCRIPT,
                ScriptType.DatabaseUpdate => UPDATE_DATABASE_SCRIPT,
                ScriptType.RestoreContinuousIntegration => CI_RESTORE_SCRIPT,
                ScriptType.StoreContinuousIntegration => CI_STORE_SCRIPT,
                _ => String.Empty,
            };

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
        /// An invalid script type.
        /// </summary>
        None,


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
        BuildProject,


        /// <summary>
        /// The script which stores Continuous Intgeration data on the filesystem.
        /// </summary>
        StoreContinuousIntegration,


        /// <summary>
        /// The script which restores Continuous Intgeration data to the database.
        /// </summary>
        RestoreContinuousIntegration
    }
}
