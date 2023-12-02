﻿using Spectre.Console;

using Xperience.Xman.Helpers;
using Xperience.Xman.Options;
using Xperience.Xman.Wizards;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which updates the NuGet packages and database of the Xperience by Kentico project in
    /// the current directory.
    /// </summary>
    public class UpdateCommand : AbstractCommand
    {
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


        public override void Execute(string[] args)
        {
            var options = new UpdateWizard().Run();

            AnsiConsole.WriteLine();
            UpdatePackages(options);
            BuildProject();
            // There is currently an issue running the database update script while emulating the ReadKey() input
            // for the script's "Do you want to continue" prompt. The update command must be run manually and the
            // UpdateDatabase method is skipped.
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Unfortunately, the database cannot be updated at this time. Please run the 'dotnet run --no-build --kxp-update' command manually.[/]");
        }


        private void UpdatePackages(UpdateOptions options)
        {
            foreach (var package in packageNames)
            {
                if (StopProcessing) return;

                var message = options.Version is null ? $"Updating {package} package to the latest version..." : $"Updating {package} package to version {options.Version}...";
                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]{message}[/]");

                options.PackageName = package;
                var packageScript = new ScriptBuilder(ScriptType.PackageUpdate).WithOptions(options).AppendVersion(options.Version).Build();
                var packageCmd = CommandHelper.ExecuteShell(packageScript);
                packageCmd.ErrorDataReceived += ErrorDataReceived;
                packageCmd.BeginErrorReadLine();
                packageCmd.WaitForExit();
            }
        }


        private void BuildProject()
        {
            if (StopProcessing) return;

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Attempting to build the project...[/]");

            var buildScript = new ScriptBuilder(ScriptType.BuildProject).Build();
            var buildCmd = CommandHelper.ExecuteShell(buildScript);
            buildCmd.ErrorDataReceived += ErrorDataReceived;
            buildCmd.BeginErrorReadLine();
            buildCmd.WaitForExit();
        }


        private void UpdateDatabase()
        {
            if (StopProcessing) return;

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updating the database...[/]");

            var databaseScript = new ScriptBuilder(ScriptType.DatabaseUpdate).Build();
            var databaseCmd = CommandHelper.ExecuteShell(databaseScript, true);
            databaseCmd.ErrorDataReceived += ErrorDataReceived;
            databaseCmd.OutputDataReceived += (o, e) =>
            {
                Console.WriteLine(e.Data);
                if (e.Data?.Contains("Do you want to continue", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Bypass backup warning
                    databaseCmd.StandardInput.WriteLine('Y');
                }
            };
            databaseCmd.BeginErrorReadLine();
            databaseCmd.BeginOutputReadLine();
            databaseCmd.WaitForExit();
        }
    }
}
