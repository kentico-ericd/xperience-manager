using Spectre.Console;

using System.Diagnostics;

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
        private readonly Configuration.Profile profile = new();
        private readonly IShellRunner shellRunner;
        private readonly IConfigManager configManager;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IWizard<InstallOptions> wizard;


        public override IEnumerable<string> Keywords => new string[] { "i", "install" };


        public override IEnumerable<string> Parameters => Array.Empty<string>();


        public override string Description => "Installs a new XbK instance";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
        internal InstallCommand()
        {
        }


        public InstallCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IWizard<InstallOptions> wizard, IConfigManager configManager)
        {
            this.wizard = wizard;
            this.shellRunner = shellRunner;
            this.configManager = configManager;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task Execute(string[] args)
        {
            // Override default values of InstallOptions with values from config file
            wizard.Options = await configManager.GetDefaultInstallOptions();
            var options = await wizard.Run();

            profile.ProjectName = options.ProjectName;
            profile.WorkingDirectory = Path.GetFullPath(options.ProjectName);

            AnsiConsole.WriteLine();
            await CreateWorkingDirectory(options);
            await InstallTemplate(options);
            await CreateProjectFiles(options);
            await CreateDatabase(options);
            if (!Errors.Any())
            {
                await configManager.AddProfile(profile);
            }
        }


        private async Task CreateWorkingDirectory(InstallOptions options)
        {
            string mkdirScript = scriptBuilder.SetScript(ScriptType.CreateDirectory).WithOptions(options).Build();
            await shellRunner.Execute(new(mkdirScript) { ErrorHandler = ErrorDataReceived }).WaitForExitAsync();
        }


        private async Task CreateDatabase(InstallOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running database creation script...[/]");

            string databaseScript = scriptBuilder.SetScript(ScriptType.DatabaseInstall).WithOptions(options).Build();
            await shellRunner.Execute(new(databaseScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = profile.WorkingDirectory
            }).WaitForExitAsync();
        }


        private async Task CreateProjectFiles(InstallOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running project creation script...[/]");

            string installScript = scriptBuilder.SetScript(ScriptType.ProjectInstall)
                .WithOptions(options)
                .AppendCloud(options.UseCloud)
                .Build();
            await shellRunner.Execute(new(installScript)
            {
                KeepOpen = true,
                WorkingDirectory = profile.WorkingDirectory,
                ErrorHandler = ErrorDataReceived,
                OutputHandler = (o, e) =>
                {
                    var proc = o as Process;
                    if (e.Data?.Contains("Do you want to run this action", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // Restore packages when prompted
                        proc?.StandardInput.WriteLine("Y");
                        proc?.StandardInput.Close();
                    }
                }
            }).WaitForExitAsync();
        }


        private async Task InstallTemplate(InstallOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Uninstalling previous template version...[/]");

            string uninstallScript = scriptBuilder.SetScript(ScriptType.TemplateUninstall).Build();
            // Don't use base error handler for uninstall script as it throws when no templates are installed
            // Just skip uninstall step in case of error and try to continue
            var uninstallCmd = shellRunner.Execute(new(uninstallScript));
            await uninstallCmd.WaitForExitAsync();

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Installing template version {options.Version}...[/]");

            string installScript = scriptBuilder.SetScript(ScriptType.TemplateInstall)
                .WithOptions(options)
                .AppendVersion(options.Version)
                .Build();
            var installCmd = shellRunner.Execute(new(installScript) { ErrorHandler = ErrorDataReceived });
            await installCmd.WaitForExitAsync();
        }
    }
}
