using Spectre.Console;

using System.Diagnostics;

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
        private readonly IScriptBuilder scriptBuilder;


        public override IEnumerable<string> Keywords => new string[] { "i", "install" };


        public override IEnumerable<string> Parameters => Array.Empty<string>();


        public override string Description => "Installs a new XbK instance";


        public InstallCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder)
        {
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
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

            var databaseScript = scriptBuilder.SetScript(ScriptType.DatabaseInstall).WithOptions(options).Build();
            shellRunner.Execute(databaseScript, ErrorDataReceived).WaitForExit();
        }


        private void CreateProjectFiles(InstallOptions options)
        {
            if (StopProcessing) return;

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running project creation script...[/]");

            var installScript = scriptBuilder.SetScript(ScriptType.ProjectInstall)
                .WithOptions(options)
                .AppendCloud(options.UseCloud)
                .Build();
            shellRunner.Execute(installScript, ErrorDataReceived, ProjectFilesOutputReceived, true).WaitForExit();
        }


        private void InstallTemplate(InstallOptions options)
        {
            if (StopProcessing) return;

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Uninstalling previous template version...[/]");

            var uninstallScript = scriptBuilder.SetScript(ScriptType.TemplateUninstall).Build();
            // Don't use base error handler for uninstall script as it throws when no templates are installed
            // Just skip uninstall step in case of error and try to continue
            var uninstallCmd = shellRunner.Execute(uninstallScript);
            uninstallCmd.WaitForExit();

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Installing template version {options.Version}...[/]");

            var installScript = scriptBuilder.SetScript(ScriptType.TemplateInstall)
                .WithOptions(options)
                .AppendVersion(options.Version)
                .Build();
            var installCmd = shellRunner.Execute(installScript, ErrorDataReceived);
            installCmd.WaitForExit();
        }


        private void ProjectFilesOutputReceived(object sender, DataReceivedEventArgs e)
        {
            var proc = sender as Process;
            if (e.Data?.Contains("Do you want to run this action", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                // Restore packages when prompted
                proc?.StandardInput.WriteLine("Y");
            }
            else if (e.Data?.Contains("Restore was successful", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                // Workaround for the installation process staying open forever
                proc?.StandardInput.Close();
                proc?.Close();
            }
        }
    }
}