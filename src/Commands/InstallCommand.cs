using Spectre.Console;

using Xperience.Xman.Helpers;
using Xperience.Xman.Options;
using Xperience.Xman.Services;
using Xperience.Xman.Wizards;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which installs new Xperience by Kentico project files and database in the current directory.
    /// </summary>
    public class InstallCommand : AbstractCommand
    {
        private readonly IShellRunner shellRunner;


        public override IEnumerable<string> Keywords => new string[] { "i", "install" };


        public override IEnumerable<string> Parameters => Array.Empty<string>();


        public override string Description => "Installs a new XbK instance";


        public InstallCommand(IShellRunner shellRunner)
        {
            this.shellRunner = shellRunner;
        }


        public override void Execute(string[] args)
        {
            InstallOptions? options = ConfigFileHelper.GetOptionsFromConfig();
            if (options is null)
            {
                options = new InstallWizard().Run();
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Configuration loaded from file, proceeding with install...[/]");
            }

            AnsiConsole.WriteLine();
            InstallTemplate(options);
            CreateProjectFiles(options);
            CreateDatabase(options);
            if (!Errors.Any())
            {
                ConfigFileHelper.CreateConfigFile(options);
            }
        }


        private void CreateDatabase(InstallOptions options)
        {
            if (StopProcessing) return;

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running database creation script...[/]");

            var databaseScript = new ScriptBuilder(ScriptType.DatabaseInstall).WithOptions(options).Build();
            shellRunner.Execute(databaseScript, ErrorDataReceived).WaitForExit();
        }


        private void CreateProjectFiles(InstallOptions options)
        {
            if (StopProcessing) return;

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running project creation script...[/]");

            var installComplete = false;
            var installScript = new ScriptBuilder(ScriptType.ProjectInstall)
                .WithOptions(options)
                .AppendCloud(options.UseCloud)
                .Build();
            var installCmd = shellRunner.Execute(installScript, ErrorDataReceived);
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
            while (!installComplete)
            {
                installCmd.WaitForExit(100);
            }
            installCmd.Close();
        }


        private void InstallTemplate(InstallOptions options)
        {
            if (StopProcessing) return;

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Uninstalling previous template version...[/]");

            var uninstallScript = new ScriptBuilder(ScriptType.TemplateUninstall).Build();
            shellRunner.Execute(uninstallScript, ErrorDataReceived).WaitForExit();

            var message = options.Version is null ? "Installing latest template version..." : $"Installing template version {options.Version}...";
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]{message}[/]");

            var installScript = new ScriptBuilder(ScriptType.TemplateInstall)
                .WithOptions(options)
                .AppendVersion(options.Version)
                .Build();
            var installCmd = shellRunner.Execute(installScript, ErrorDataReceived);
            installCmd.WaitForExit();

            if (installCmd.ExitCode != 0)
            {
                LogError("Template installation failed. Please check version number");
            }
        }
    }
}