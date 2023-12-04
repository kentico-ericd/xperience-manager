using Spectre.Console;

using System.Diagnostics;

using Xperience.Xman.Services;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which stores or restores Continuous Integration data.
    /// </summary>
    public class ContinuousIntegrationCommand : AbstractCommand
    {
        private const string STORE = "store";
        private const string RESTORE = "restore";
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        

        public override IEnumerable<string> Keywords => new string[] { "ci" };


        public override IEnumerable<string> Parameters => new string[] { STORE, RESTORE };


        public override string Description => "Stores or restores CI data";


        public ContinuousIntegrationCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder)
        {
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
        }


        public override void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Must provide 1 parameter from '{String.Join(", ", Parameters)}'[/]");
                return;
            }

            var action = args[1].ToLower();
            if (!Parameters.Any(p => p.Equals(action, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Invalid parameter '{action}'[/]");
                return;
            }

            if (action.Equals(STORE, StringComparison.OrdinalIgnoreCase))
            {
                StoreFiles();
            }
            else if (action.Equals(RESTORE, StringComparison.OrdinalIgnoreCase))
            {
                RestoreFiles();
            }
        }


        private void StoreFiles()
        {
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running the CI store script...[/]");

            var ciScript = scriptBuilder.SetScript(ScriptType.StoreContinuousIntegration).Build();
            shellRunner.Execute(ciScript, ErrorDataReceived).WaitForExit();
        }


        private void RestoreFiles()
        {
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running the CI restore script...[/]");

            var ciScript = scriptBuilder.SetScript(ScriptType.RestoreContinuousIntegration).Build();
            shellRunner.Execute(ciScript, ErrorDataReceived, (o, e) =>
            {
                if (e.Data?.Contains("The Continuous Integration repository is either not initialized or in an incorrect location on the file system.", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Restore process couldn't find repository directory
                    LogError("The restore process wasn't started because the Continuous Integration repository wasn't found.", o as Process);
                }
            }).WaitForExit();
        }
    }
}
