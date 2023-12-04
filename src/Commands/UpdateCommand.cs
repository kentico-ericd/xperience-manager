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


        public UpdateCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder)
        {
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
        }


        public override void Execute(string[] args)
        {
            var options = new UpdateWizard().Run();

            AnsiConsole.WriteLine();
            UpdatePackages(options);
            BuildProject();
            // There is currently an issue running the database update script while emulating the ReadKey() input
            // for the script's "Do you want to continue" prompt. The update command must be run manually.
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Unfortunately, the database cannot be updated at this time. Please run the 'dotnet run --no-build --kxp-update' command manually.[/]");
        }


        private void UpdatePackages(UpdateOptions options)
        {
            foreach (var package in packageNames)
            {
                if (StopProcessing) return;

                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updating {package} to version {options.Version}...[/]");

                options.PackageName = package;
                var packageScript = scriptBuilder.SetScript(ScriptType.PackageUpdate).WithOptions(options).AppendVersion(options.Version).Build();
                shellRunner.Execute(packageScript, ErrorDataReceived).WaitForExit();
            }
        }


        private void BuildProject()
        {
            if (StopProcessing) return;

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Attempting to build the project...[/]");

            var buildScript = scriptBuilder.SetScript(ScriptType.BuildProject).Build();
            shellRunner.Execute(buildScript, ErrorDataReceived).WaitForExit();
        }
    }
}
