using Spectre.Console;

using Xperience.Xman.Helpers;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which stores or restores Continuous Integration data.
    /// </summary>
    public class ContinuousIntegrationCommand : AbstractCommand
    {
        private const string STORE = "store";
        private const string RESTORE = "restore";
        

        public override IEnumerable<string> Keywords => new string[] { "ci" };


        public override IEnumerable<string> Parameters => new string[] { STORE, RESTORE };


        public override string Description => "Stores or restores CI data";


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

            var storeStarted = false;
            var ciScript = new ScriptBuilder(ScriptType.StoreContinuousIntegration).Build();
            var ciCmd = CommandHelper.ExecuteShell(ciScript);
            ciCmd.ErrorDataReceived += ErrorDataReceived;
            ciCmd.OutputDataReceived += (o, e) =>
            {
                if (e.Data?.Contains("Storing objects...", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Mark process started, since running the store command without CI enabled in Settings doesn't throw error
                    storeStarted = true;
                }
            };
            ciCmd.BeginOutputReadLine();
            ciCmd.BeginErrorReadLine();
            ciCmd.WaitForExit();
            if (!storeStarted)
            {
                LogError("The store process wasn't started. This is most likely due to Continuous Integration being disabled in Settings.");
            }
        }


        private void RestoreFiles()
        {
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running the CI restore script...[/]");

            var ciScript = new ScriptBuilder(ScriptType.RestoreContinuousIntegration).Build();
            var ciCmd = CommandHelper.ExecuteShell(ciScript);
            ciCmd.ErrorDataReceived += ErrorDataReceived;
            ciCmd.OutputDataReceived += (o, e) =>
            {
                if (e.Data?.Contains("The Continuous Integration repository is either not initialized or in an incorrect location on the file system.", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Restore process couldn't find repository directory
                    StopProcessing = true;
                    LogError("The restore process wasn't started because the Continuous Integration repository wasn't found.");
                }
            };
            ciCmd.BeginOutputReadLine();
            ciCmd.BeginErrorReadLine();
            ciCmd.WaitForExit();
        }
    }
}
