using System.Diagnostics;

using Xperience.Xman.Helpers;
using Xperience.Xman.Models;

namespace Xperience.Xman.Commands
{
    public class InstallCommand : Command
    {
        private static readonly string[] KEYWORDS = new string[] { "install" };
        private static readonly string DESCRIPTION = "Installs a new XbK instance";


        public InstallCommand() : base(KEYWORDS, DESCRIPTION)
        {
        }


        public override void Execute()
        {
            var options = InstallHelper.GetOptions();
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

            var databaseCmd = CommandHelper.ExecuteShell($"dotnet kentico-xperience-dbmanager -- -s \"{options.ServerName}\" -d \"{options.DatabaseName}\" -a \"{options.AdminPassword}\"");
            databaseCmd.ErrorDataReceived += ErrorRecieved;
            databaseCmd.BeginErrorReadLine();
            databaseCmd.WaitForExit();
        }


        private void CreateProjectFiles(InstallOptions options)
        {
            Console.WriteLine("Running project creation script...");

            var installComplete = false;
            var installCmd = CommandHelper.ExecuteShell($"dotnet new {options.Template} -n {options.ProjectName} {(options.UseCloud ? "--cloud" : string.Empty)}", true);
            installCmd.ErrorDataReceived += ErrorRecieved;
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
            CommandHelper.ExecuteShell("dotnet new uninstall kentico.xperience.templates").WaitForExit();
            if (options.Version.Major == 0)
            {
                Console.WriteLine("Installing latest template version...");
                CommandHelper.ExecuteShell("dotnet new install kentico.xperience.templates").WaitForExit();
            }
            else
            {
                Console.WriteLine($"Installing template version {options.Version}...");
                CommandHelper.ExecuteShell($"dotnet new install kentico.xperience.templates::{options.Version}").WaitForExit();
            }
        }


        private void ErrorRecieved(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}