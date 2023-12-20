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
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IWizard<UpdateOptions> wizard;
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


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal UpdateCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public UpdateCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IWizard<UpdateOptions> wizard)
        {
            this.wizard = wizard;
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task PreExecute(ToolProfile? profile, string[] args)
        {
            await base.PreExecute(profile, args);

            options = await wizard.Run();
        }


        public override async Task Execute(ToolProfile? profile, string[] args)
        {
            if (options is null)
            {
                throw new InvalidOperationException("Options weren't found.");
            }

            await UpdatePackages(options, profile);
            // There is currently an issue running the database update script while emulating the ReadKey() input
            // for the script's "Do you want to continue" prompt. The update command must be run manually.
            AnsiConsole.MarkupLineInterpolated($"Unfortunately, the database cannot be updated at this time. You can build the project and run the [{Constants.EMPHASIS_COLOR}]'dotnet run --no-build --kxp-update'[/] command manually if needed.");
        }


        public override async Task PostExecute(ToolProfile? profile, string[] args)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Update complete![/]\n");
            }

            await base.PostExecute(profile, args);
        }


        private async Task UpdatePackages(UpdateOptions options, ToolProfile? profile)
        {
            foreach (string package in packageNames)
            {
                if (StopProcessing)
                {
                    return;
                }

                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updating {package} to version {options.Version}...[/]");

                options.PackageName = package;
                string packageScript = scriptBuilder.SetScript(ScriptType.PackageUpdate)
                    .WithPlaceholders(options)
                    .AppendVersion(options.Version)
                    .Build();
                await shellRunner.Execute(new(packageScript)
                {
                    ErrorHandler = ErrorDataReceived,
                    WorkingDirectory = profile?.WorkingDirectory
                }).WaitForExitAsync();
            }
        }
    }
}
