using System.Diagnostics;

using Xperience.Xman.Helpers;
using Xperience.Xman.Models;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which installs new Xperience by Kentico project files and database in the current directory.
    /// </summary>
    public class InstallCommand : ICommand
    {
        private bool stopProcessing = false;
        private readonly IList<string> errors = new List<string>();


        public IEnumerable<string> Keywords => new string[] { "i", "install" };


        public string Description => "Installs a new XbK instance";


        public void Execute()
        {
            InstallOptions? options = ConfigFileHelper.GetOptionsFromConfig();
            if (options is null)
            {
                options = InstallOptionsHelper.GetOptions();
            }
            else
            {
                XConsole.WriteSuccessLine("Configuration loaded from file, proceeding with install...");
            }

            try
            {
                InstallTemplate(options);
                CreateProjectFiles(options);
                CreateDatabase(options);
                if (errors.Any())
                {
                    XConsole.WriteErrorLine($"Installation failed with errors:\n{String.Join("\n", errors)}");
                }
                else
                {
                    XConsole.WriteSuccessLine("Installation complete!");
                    ConfigFileHelper.CreateConfigFile(options);
                }
            }
            catch (Exception e)
            {
                XConsole.WriteErrorLine($"Installation failed with the error: {e.Message}");
            }
        }


        private void CreateDatabase(InstallOptions options)
        {
            if (stopProcessing) return;

            XConsole.WriteEmphasisLine("Running database creation script...");

            var databaseScript = new ScriptBuilder(ScriptType.DatabaseInstall).WithOptions(options).Build();
            var databaseCmd = CommandHelper.ExecuteShell(databaseScript);
            databaseCmd.ErrorDataReceived += ErrorDataReceived;
            databaseCmd.BeginErrorReadLine();
            databaseCmd.WaitForExit();
        }


        private void CreateProjectFiles(InstallOptions options)
        {
            if (stopProcessing) return;

            XConsole.WriteEmphasisLine("Running project creation script...");

            var installComplete = false;
            var installScript = new ScriptBuilder(ScriptType.ProjectInstall).WithOptions(options).Build();
            var installCmd = CommandHelper.ExecuteShell(installScript, true);
            installCmd.ErrorDataReceived += ErrorDataReceived;
            installCmd.OutputDataReceived += (o, e) =>
            {
                if (e.Data?.Contains("Do you want to run this action", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Restore packages when prompted
                    installCmd.StandardInput.WriteLine("Y");
                }
                else if (e.Data?.Contains("Restore was successful", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Workaround for the installation process staying open forever
                    installComplete = true;
                }
            };
            installCmd.BeginErrorReadLine();
            installCmd.BeginOutputReadLine();
            while (!installComplete)
            {
                installCmd.WaitForExit(100);
            }
            installCmd.Close();
        }


        private void InstallTemplate(InstallOptions options)
        {
            if (stopProcessing) return;

            XConsole.WriteEmphasisLine("Uninstalling previous template version...");

            var uninstallScript = new ScriptBuilder(ScriptType.TemplateUninstall).Build();
            CommandHelper.ExecuteShell(uninstallScript).WaitForExit();

            var message = options.Version is null ? "Installing latest template version..." : $"Installing template version {options.Version}...";
            XConsole.WriteEmphasisLine(message);

            var installScript = new ScriptBuilder(ScriptType.TemplateInstall).WithOptions(options).Build();
            var installCmd = CommandHelper.ExecuteShell(installScript);
            installCmd.WaitForExit();
            if (installCmd.ExitCode != 0)
            {
                LogError("Template installation failed. Please check version number");
            }
        }


        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                LogError(e.Data);
            }
        }


        private void LogError(string message)
        {
            stopProcessing = true;
            errors.Add(message);
        }
    }
}