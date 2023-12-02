using Spectre.Console;

using System.Diagnostics;

using Xperience.Xman.Helpers;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which stores or restores Continuous Integration data.
    /// </summary>
    public class ContinuousIntegrationCommand : ICommand
    {
        private const string STORE = "store";
        private const string RESTORE = "restore";
        private readonly List<string> errors = new();


        public List<string> Errors => errors;


        public bool StopProcessing { get; set; }


        public IEnumerable<string> Keywords => new string[] { "ci" };


        public IEnumerable<string> Parameters => new string[] { STORE, RESTORE };


        public string Description => "Stores or restores CI data";


        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Must provide 1 parameter from '{String.Join(", ", Parameters)}'[/]");
                return;
            }

            var parameter = args[1].ToLower();
            if (!Parameters.Any(p => p.Equals(parameter, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Invalid parameter '{parameter}'[/]");
                return;
            }

            ScriptType scriptType = parameter switch
            {
                STORE => ScriptType.StoreContinuousIngration,
                RESTORE => ScriptType.RestoreContinuousIngration,
                _ => ScriptType.None,
            };

            var storeStarted = false;
            var actionType = scriptType.Equals(ScriptType.StoreContinuousIngration) ? "store" : "restore";
            try
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Running the CI {actionType} script...[/]");

                var ciScript = new ScriptBuilder(scriptType).Build();
                var ciCmd = CommandHelper.ExecuteShell(ciScript);
                ciCmd.ErrorDataReceived += ErrorDataReceived;
                ciCmd.OutputDataReceived += (o, e) =>
                {
                    if (e.Data?.Contains("Storing objects...", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // Mark process started, since running the store command without CI enabled in Settings doesn't throw error
                        storeStarted = true;
                    }
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

                if (scriptType.Equals(ScriptType.StoreContinuousIngration) && !storeStarted)
                {
                    LogError("The store process wasn't started. This is most likely due to Continuous Integration being disabled in Settings.");
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }
        }


        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                LogError(e.Data);
            }
        }


        private void LogError(string message)
        {
            StopProcessing = true;
            Errors.Add(message);
        }
    }
}
