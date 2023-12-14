using System.Diagnostics;

using Spectre.Console;

using Xperience.Xman.Configuration;
using Xperience.Xman.Services;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which manages Continuous Deployment profiles and stores/restores data.
    /// </summary>
    public class ContinuousDeploymentCommand : AbstractCommand
    {
        private string? actionName;
        private ToolProfile? profile;
        private const string STORE = "store";
        private const string RESTORE = "restore";
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IConfigManager configManager;


        public override IEnumerable<string> Keywords => new string[] { "cd" };


        public override IEnumerable<string> Parameters => new string[] { STORE, RESTORE };


        public override string Description => "Stores or restores CD data";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal ContinuousDeploymentCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public ContinuousDeploymentCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IConfigManager configManager)
        {
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
            this.configManager = configManager;
        }


        public override async Task PreExecute(string[] args)
        {
            if (args.Length < 2)
            {
                throw new InvalidOperationException($"Must provide 1 parameter from '{string.Join(", ", Parameters)}'");
            }

            actionName = args[1].ToLower();
            if (!Parameters.Any(p => p.Equals(actionName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Invalid parameter '{actionName}'");
            }

            profile = await configManager.GetCurrentProfile() ?? throw new InvalidOperationException("There is no active profile.");
            PrintCurrentProfile(profile);

            await base.PreExecute(args);
        }


        public override async Task Execute(string[] args)
        {
            if (profile is null)
            {
                return;
            }

            if (actionName?.Equals(STORE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await EnsureCDStructure(profile);
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
                        var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Running the CD store script[/]");
                        await StoreFiles(task, profile);
                    });
            }
            else if (actionName?.Equals(RESTORE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var sourceProfile = await GetSourceProfile();
                if (sourceProfile is null)
                {
                    return;
                }

                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                    {
                        new SpinnerColumn(),
                        new ElapsedTimeColumn(),
                        new TaskDescriptionColumn()
                    })
                    .StartAsync(async ctx =>
                    {
                        var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Running the CD restore script[/]");
                        await RestoreFiles(task, profile, sourceProfile);
                    });
            }
        }


        public override async Task PostExecute(string[] args)
        {
            if (!Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]CD {actionName ?? "process"} complete![/]\n");
            }

            await base.PostExecute(args);
        }


        private async Task<ToolProfile?> GetSourceProfile()
        {
            if (StopProcessing)
            {
                return null;
            }

            var config = await configManager.GetConfig();
            var profiles = config.Profiles.Where(p => !p.ProjectName?.Equals(profile.ProjectName, StringComparison.OrdinalIgnoreCase) ?? false);
            if (!profiles.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"There are no profiles to restore CD data from. Use the [{Constants.EMPHASIS_COLOR}]install[/] or [{Constants.EMPHASIS_COLOR}]profile add[/] commands to register a new profile.");
                StopProcessing = true;
                return null;
            }

            var prompt = new SelectionPrompt<ToolProfile>()
                    .Title("Restore data from which profile?")
                    .PageSize(10)
                    .UseConverter(p => p.ProjectName ?? string.Empty)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(profiles);

            return AnsiConsole.Prompt(prompt);
        }


        private async Task EnsureCDStructure(ToolProfile profile)
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(profile.RepositoryPath) || string.IsNullOrEmpty(profile.ConfigPath))
            {
                throw new InvalidOperationException("Unable to load profile's repository configuration.");
            }

            Directory.CreateDirectory(profile.RepositoryPath);

            if (!File.Exists(profile.ConfigPath))
            {
                string cdScript = scriptBuilder.SetScript(ScriptType.ContinuousDeploymentNewConfiguration).WithPlaceholders(profile).Build();
                await shellRunner.Execute(new(cdScript)
                {
                    ErrorHandler = ErrorDataReceived,
                    WorkingDirectory = profile.WorkingDirectory
                }).WaitForExitAsync();
            }
        }


        private async Task RestoreFiles(ProgressTask task, ToolProfile profile, ToolProfile sourceProfile)
        {
            if (StopProcessing)
            {
                return;
            }

            string originalDescription = task.Description;
            string cdScript = scriptBuilder.SetScript(ScriptType.ContinuousDeploymentRestore).WithPlaceholders(sourceProfile).Build();
            await shellRunner.Execute(new(cdScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = profile.WorkingDirectory,
                OutputHandler = (o, e) =>
                {
                    if (e.Data?.Contains("Object type", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // Message is something like "Object type Module: updating Activities"
                        task.Description = e.Data;
                    }
                }
            }).WaitForExitAsync();

            task.Description = originalDescription;
        }


        private async Task StoreFiles(ProgressTask task, ToolProfile profile)
        {
            if (StopProcessing)
            {
                return;
            }

            string cdScript = scriptBuilder.SetScript(ScriptType.ContinuousDeploymentStore).WithPlaceholders(profile).Build();
            await shellRunner.Execute(new(cdScript)
            {
                ErrorHandler = ErrorDataReceived,
                OutputHandler = (o, e) =>
                {
                    if (e.Data?.Contains("System.IO.IOException", StringComparison.OrdinalIgnoreCase) ?? false)
                    {
                        // For some reason, System.IO.IOException is not caught by the error handler
                        LogError(e.Data, o as Process);
                    }
                    else if ((e.Data?.Contains("Object type", StringComparison.OrdinalIgnoreCase) ?? false) && e.Data.Any(char.IsDigit))
                    {
                        // Message is something like "Object type 1/84: Module"
                        string[] progressMessage = e.Data.Split(':');
                        if (progressMessage.Length == 0)
                        {
                            return;
                        }

                        string[] progressNumbers = progressMessage[0].Split('/');
                        if (progressNumbers.Length < 2)
                        {
                            return;
                        }

                        double progressCurrent = double.Parse(string.Join("", progressNumbers[0].Where(char.IsDigit)));
                        double progressMax = double.Parse(string.Join("", progressNumbers[1].Where(char.IsDigit)));

                        task.MaxValue = progressMax;
                        task.Value = progressCurrent;
                    }
                },
                WorkingDirectory = profile.WorkingDirectory
            }).WaitForExitAsync();
        }
    }
}
