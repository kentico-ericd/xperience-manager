using Spectre.Console;

using System.Diagnostics;

using Xperience.Xman.Configuration;
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
        private InstallOptions? options;
        private readonly ToolProfile profile = new();
        private readonly IShellRunner shellRunner;
        private readonly IConfigManager configManager;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IWizard<InstallOptions> wizard;


        public override IEnumerable<string> Keywords => new string[] { "i", "install" };


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Installs a new XbK instance";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal InstallCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public InstallCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IWizard<InstallOptions> wizard, IConfigManager configManager)
        {
            this.wizard = wizard;
            this.shellRunner = shellRunner;
            this.configManager = configManager;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task PreExecute(string[] args)
        {
            // Override default values of InstallOptions with values from config file
            wizard.Options = await configManager.GetDefaultInstallOptions();
            options = await wizard.Run();
            AnsiConsole.WriteLine();

            profile.ProjectName = options.ProjectName;
            profile.WorkingDirectory = $"{options.InstallRootPath}\\{options.ProjectName}";

            await base.PreExecute(args);
        }


        public override async Task Execute(string[] args)
        {
            if (options is null)
            {
                throw new InvalidOperationException("The installation options weren't found.");
            }

            await CreateWorkingDirectory();
            await InstallTemplate(options);
            await CreateProjectFiles(options);
            // Admin boilerplate project doesn't require database install
            if (!IsAdminTemplate())
            {
                await CreateDatabase(options);
            }

            // Don't create profile for admin boilerplate since it's meant to be moved/included in another installation
            if (!IsAdminTemplate())
            {
                await configManager.AddProfile(profile);
            }
        }


        public override async Task PostExecute(string[] args)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Install complete![/]\n");
            }

            await base.PostExecute(args);
        }


        private async Task CreateWorkingDirectory()
        {
            string mkdirScript = scriptBuilder.SetScript(ScriptType.CreateDirectory)
                .AppendDirectory(profile.WorkingDirectory)
                .Build();
            await shellRunner.Execute(new(mkdirScript) { ErrorHandler = ErrorDataReceived }).WaitForExitAsync();
        }


        private async Task CreateDatabase(InstallOptions options)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running database creation script...[/]");

            string databaseScript = scriptBuilder.SetScript(ScriptType.DatabaseInstall)
                .WithPlaceholders(options)
                .Build();
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
                .WithPlaceholders(options)
                .AppendCloud(options.UseCloud)
                .Build();

            // Admin boilerplate script doesn't require input
            bool keepOpen = !IsAdminTemplate();
            await shellRunner.Execute(new(installScript)
            {
                KeepOpen = keepOpen,
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
                .WithPlaceholders(options)
                .AppendVersion(options.Version)
                .Build();
            var installCmd = shellRunner.Execute(new(installScript) { ErrorHandler = ErrorDataReceived });
            await installCmd.WaitForExitAsync();
        }


        private bool IsAdminTemplate() => options?.Template.Equals(Constants.TEMPLATE_ADMIN, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
