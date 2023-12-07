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


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
        internal ContinuousIntegrationCommand()
        {
        }


        public ContinuousIntegrationCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder)
        {
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
        }


        public override async Task Execute(string[] args)
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
                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                    {
                        new SpinnerColumn(),
                        new ElapsedTimeColumn(),
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn(),
                        new PercentageColumn()
                    })
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Running the CI store script[/]");
                        await StoreFiles(task);
                    });
            }
            else if (action.Equals(RESTORE, StringComparison.OrdinalIgnoreCase))
            {
                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                    {
                        new SpinnerColumn(),
                        new ElapsedTimeColumn(),
                        new TaskDescriptionColumn()
                    })
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Running the CI restore script[/]");
                        await RestoreFiles(task);
                    });
            }
        }


        private async Task StoreFiles(ProgressTask task)
        {
            var ciScript = scriptBuilder.SetScript(ScriptType.StoreContinuousIntegration).Build();
            await shellRunner.Execute(ciScript, ErrorDataReceived, (o, e) => {
                if (e.Data?.Contains("Object type", StringComparison.OrdinalIgnoreCase) ?? false && e.Data.Any(char.IsDigit))
                {
                    // Message is something like "Object type 1/84: Module"
                    var progressMessage = e.Data.Split(':');
                    if (progressMessage.Length == 0) return;

                    var progressNumbers = progressMessage[0].Split('/');
                    if (progressNumbers.Length < 2) return;

                    var progressCurrent = double.Parse(String.Join("", progressNumbers[0].Where(char.IsDigit)));
                    var progressMax = double.Parse(String.Join("", progressNumbers[1].Where(char.IsDigit)));

                    task.MaxValue = progressMax;
                    task.Value = progressCurrent;
                }
            }).WaitForExitAsync();
        }


        private async Task RestoreFiles(ProgressTask task)
        {
            var originalDescription = task.Description;
            var ciScript = scriptBuilder.SetScript(ScriptType.RestoreContinuousIntegration).Build();
            await shellRunner.Execute(ciScript, ErrorDataReceived, (o, e) =>
            {
                if (e.Data?.Contains("The Continuous Integration repository is either not initialized or in an incorrect location on the file system.", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Restore process couldn't find repository directory
                    LogError("The restore process wasn't started because the Continuous Integration repository wasn't found.", o as Process);
                }
                else if (e.Data?.Contains("Object type", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Message is something like "Object type Module: updating Activities"
                    task.Description = e.Data;
                }
            }).WaitForExitAsync();

            task.Description = originalDescription;
        }
    }
}
