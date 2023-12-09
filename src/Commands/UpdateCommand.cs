using Spectre.Console;

using Xperience.Xman.Configuration;
using Xperience.Xman.Options;
using Xperience.Xman.Services;
using Xperience.Xman.Wizards;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which updates the NuGet packages and database of the Xperience by Kentico project in
    /// the current directory.
    /// </summary>
    public class UpdateCommand : AbstractCommand
    {
        private UpdateOptions? options;
        private ToolProfile? profile;
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IWizard<UpdateOptions> wizard;
        private readonly IConfigManager configManager;
        private readonly IEnumerable<string> packageNames = new string[]
        {
            "kentico.xperience.admin",
            "kentico.xperience.azurestorage",
            "kentico.xperience.cloud",
            "kentico.xperience.graphql",
            "kentico.xperience.imageprocessing",
            "kentico.xperience.webapp"
        };


        public override IEnumerable<string> Keywords => new string[] { "u", "update" };


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Updates a project's NuGet packages and database version";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
        internal UpdateCommand()
        {
        }


        public UpdateCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IWizard<UpdateOptions> wizard, IConfigManager configManager)
        {
            this.wizard = wizard;
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
            this.configManager = configManager;
        }


        public override async Task PreExecute(string[] args)
        {
            profile = await configManager.GetCurrentProfile() ?? throw new InvalidOperationException("There is no active profile.");
            PrintCurrentProfile(profile);

            options = await wizard.Run();

            AnsiConsole.WriteLine();
        }


        public override async Task Execute(string[] args)
        {
            if (options is null || profile is null)
            {
                throw new InvalidOperationException("");
            }

            await UpdatePackages(options, profile);
            await BuildProject(profile);
            // There is currently an issue running the database update script while emulating the ReadKey() input
            // for the script's "Do you want to continue" prompt. The update command must be run manually.
            AnsiConsole.MarkupLineInterpolated($"Unfortunately, the database cannot be updated at this time. Please run the [{Constants.SUCCESS_COLOR}]'dotnet run --no-build --kxp-update'[/] command manually.");
        }


        public override async Task PostExecute(string[] args)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Update complete![/]\n");
            }

            await base.PostExecute(args);
        }


        private async Task UpdatePackages(UpdateOptions options, ToolProfile profile)
        {
            foreach (string package in packageNames)
            {
                if (StopProcessing)
                {
                    return;
                }

                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updating {package} to version {options.Version}...[/]");

                options.PackageName = package;
                string packageScript = scriptBuilder.SetScript(ScriptType.PackageUpdate).WithOptions(options).AppendVersion(options.Version).Build();
                await shellRunner.Execute(new(packageScript)
                {
                    ErrorHandler = ErrorDataReceived,
                    WorkingDirectory = profile.WorkingDirectory
                }).WaitForExitAsync();
            }
        }


        private async Task BuildProject(ToolProfile profile)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Attempting to build the project...[/]");

            string buildScript = scriptBuilder.SetScript(ScriptType.BuildProject).Build();
            await shellRunner.Execute(new(buildScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = profile.WorkingDirectory
            }).WaitForExitAsync();
        }
    }
}
