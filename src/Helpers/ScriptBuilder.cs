using Xperience.Xman.Models;

namespace Xperience.Xman.Helpers
{
    public class ScriptBuilder
    {
        private string currentScript;
        private readonly ScriptType currentScriptType;
        private const string INSTALL_PROJECT_SCRIPT = $"dotnet new {nameof(InstallOptions.Template)} -n {nameof(InstallOptions.ProjectName)}";
        private const string INSTALL_DATABASE_SCRIPT = $"dotnet kentico-xperience-dbmanager -- -s \"{nameof(InstallOptions.ServerName)}\" -d \"{nameof(InstallOptions.DatabaseName)}\" -a \"{nameof(InstallOptions.AdminPassword)}\"";
        private const string UNINSTALL_TEMPLATE_SCRIPT = "dotnet new uninstall kentico.xperience.templates";
        private const string INSTALL_TEMPLATE_SCRIPT = "dotnet new install kentico.xperience.templates";


        public ScriptBuilder(ScriptType type)
        {
            currentScriptType = type;
            switch (type)
            {
                case ScriptType.ProjectInstall:
                    currentScript = INSTALL_PROJECT_SCRIPT;
                    break;
                case ScriptType.DatabaseInstall:
                    currentScript = INSTALL_DATABASE_SCRIPT;
                    break;
                case ScriptType.TemplateUninstall:
                    currentScript = UNINSTALL_TEMPLATE_SCRIPT;
                    break;
                case ScriptType.TemplateInstall:
                    currentScript = INSTALL_TEMPLATE_SCRIPT;
                    break;
                default:
                    currentScript = String.Empty;
                    break;
            }
        }


        public string Build()
        {
            if (!ValidateScript())
            {
                throw new InvalidOperationException("The script is empty or contains placeholder values.");
            }

            return currentScript;
        }


        public ScriptBuilder WithOptions(InstallOptions options)
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

            if (currentScriptType.Equals(ScriptType.ProjectInstall) && options.UseCloud)
            {
                currentScript += " --cloud";
            }

            if (currentScriptType.Equals(ScriptType.TemplateInstall) && options.Version is not null)
            {
                currentScript += $"::{options.Version}";
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
        ProjectInstall,
        DatabaseInstall,
        TemplateUninstall,
        TemplateInstall
    }
}
