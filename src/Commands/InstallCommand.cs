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
        public IEnumerable<string> Keywords => new string[] { "i", "install" };


        public string Description => "Installs a new XbK instance";


        public void Execute()
        {
            var options = InstallOptionsHelper.GetOptions();
            try
            {
                InstallTemplate(options);
                CreateProjectFiles(options);
                CreateDatabase(options);

                Console.WriteLine("Installation complete!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Installation failed with the error: {e.Message}");
            }
        }


        private void CreateDatabase(InstallOptions options)
        {
            Console.WriteLine("Running database creation script...");

            var databaseScript = new ScriptBuilder(ScriptType.DatabaseInstall).WithOptions(options).Build();
            var databaseCmd = CommandHelper.ExecuteShell(databaseScript);
            databaseCmd.ErrorDataReceived += ErrorReceived;
            databaseCmd.BeginErrorReadLine();
            databaseCmd.WaitForExit();
        }


        private void CreateProjectFiles(InstallOptions options)
        {
            Console.WriteLine("Running project creation script...");

            var installComplete = false;
            var installScript = new ScriptBuilder(ScriptType.ProjectInstall).WithOptions(options).Build();
            var installCmd = CommandHelper.ExecuteShell(installScript, true);
            installCmd.ErrorDataReceived += ErrorReceived;
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
            // TODO: Exit process if specified version can't be found
            Console.WriteLine("Uninstalling previous template version...");

            var uninstallScript = new ScriptBuilder(ScriptType.TemplateUninstall).Build();
            CommandHelper.ExecuteShell(uninstallScript).WaitForExit();

            var installScript = new ScriptBuilder(ScriptType.TemplateInstall).WithOptions(options).Build();
            var message = options.Version is null ? "Installing latest template version..." : $"Installing template version {options.Version}...";
            
            Console.WriteLine(message);
            CommandHelper.ExecuteShell(installScript).WaitForExit();
            
        }


        private void ErrorReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}