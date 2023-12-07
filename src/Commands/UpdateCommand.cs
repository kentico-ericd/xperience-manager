using Spectre.Console;

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


        public override IEnumerable<string> Parameters => Array.Empty<string>();


        public override string Description => "Updates a project's NuGet packages and database version";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
        internal UpdateCommand()
        {
        }


        public UpdateCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IWizard<UpdateOptions> wizard)
        {
            this.wizard = wizard;
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task Execute(string[] args)
        {
            var options = await wizard.Run();

            AnsiConsole.WriteLine();
            await UpdatePackages(options);
            await BuildProject();
            // There is currently an issue running the database update script while emulating the ReadKey() input
            // for the script's "Do you want to continue" prompt. The update command must be run manually.
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Unfortunately, the database cannot be updated at this time. Please run the 'dotnet run --no-build --kxp-update' command manually.[/]");
        }


        private async Task UpdatePackages(UpdateOptions options)
        {
            foreach (var package in packageNames)
            {
                if (StopProcessing) return;

                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updating {package} to version {options.Version}...[/]");

                options.PackageName = package;
                var packageScript = scriptBuilder.SetScript(ScriptType.PackageUpdate).WithOptions(options).AppendVersion(options.Version).Build();
                await shellRunner.Execute(packageScript, ErrorDataReceived).WaitForExitAsync();
            }
        }


        private async Task BuildProject()
        {
            if (StopProcessing) return;

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Attempting to build the project...[/]");

            var buildScript = scriptBuilder.SetScript(ScriptType.BuildProject).Build();
            await shellRunner.Execute(buildScript, ErrorDataReceived).WaitForExitAsync();
        }
    }
}
