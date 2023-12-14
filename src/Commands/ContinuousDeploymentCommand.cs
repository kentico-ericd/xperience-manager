using System.Diagnostics;

using Spectre.Console;

using Xperience.Xman.Configuration;
using Xperience.Xman.Options;
using Xperience.Xman.Services;
using Xperience.Xman.Wizards;

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
        private const string CONFIG = "config";
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IConfigManager configManager;
        private readonly ICDXmlManager cdXmlManager;
        private readonly IWizard<RepositoryConfiguration> wizard;


        public override IEnumerable<string> Keywords => new string[] { "cd" };


        public override IEnumerable<string> Parameters => new string[] { STORE, RESTORE, CONFIG };


        public override string Description => "Stores or restores CD data, or edits the config file";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal ContinuousDeploymentCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public ContinuousDeploymentCommand(IWizard<RepositoryConfiguration> wizard, ICDXmlManager cdXmlManager, IShellRunner shellRunner, IScriptBuilder scriptBuilder, IConfigManager configManager)
        {
            this.wizard = wizard;
            this.shellRunner = shellRunner;
            this.scriptBuilder = scriptBuilder;
            this.configManager = configManager;
            this.cdXmlManager = cdXmlManager;
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

            var config = await configManager.GetConfig();
            if (actionName?.Equals(CONFIG, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await ConfigureXml(config, profile);
            }
            else if (actionName?.Equals(STORE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await EnsureCDStructure(profile, config);
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
                        await StoreFiles(task, profile, config);
                    });
            }
            else if (actionName?.Equals(RESTORE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var sourceProfile = GetSourceProfile(config);
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
                        await RestoreFiles(task, profile, sourceProfile, config);
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


        private async Task ConfigureXml(ToolConfiguration toolConfig, ToolProfile profile)
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(profile.ProjectName))
            {
                throw new InvalidOperationException("Unable to load profile name.");
            }

            ContinuousDeploymentConfig cdConfig = new()
            {
                ConfigPath = Path.Combine(toolConfig.CDRootPath, profile.ProjectName, Constants.CD_CONFIG_NAME),
                RepositoryPath = Path.Combine(toolConfig.CDRootPath, profile.ProjectName, Constants.CD_FILES_DIR)
            };

            var repoConfig = await cdXmlManager.GetConfig(cdConfig.ConfigPath) ?? throw new InvalidOperationException("Unable to read repository configuration.");
            wizard.Options = repoConfig;
            var options = await wizard.Run();
            cdXmlManager.WriteConfig(options, cdConfig.ConfigPath);
        }


        private ToolProfile? GetSourceProfile(ToolConfiguration config)
        {
            if (StopProcessing)
            {
                return null;
            }

            var profiles = config.Profiles.Where(p => !p.ProjectName?.Equals(profile?.ProjectName, StringComparison.OrdinalIgnoreCase) ?? false);
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


        private async Task EnsureCDStructure(ToolProfile profile, ToolConfiguration config)
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(profile.ProjectName))
            {
                throw new InvalidOperationException("Unable to load profile name.");
            }

            ContinuousDeploymentConfig cdConfig = new()
            {
                ConfigPath = Path.Combine(config.CDRootPath, profile.ProjectName, Constants.CD_CONFIG_NAME),
                RepositoryPath = Path.Combine(config.CDRootPath, profile.ProjectName, Constants.CD_FILES_DIR)
            };

            Directory.CreateDirectory(cdConfig.RepositoryPath);

            if (!File.Exists(cdConfig.ConfigPath))
            {
                string cdScript = scriptBuilder.SetScript(ScriptType.ContinuousDeploymentNewConfiguration).WithPlaceholders(cdConfig).Build();
                await shellRunner.Execute(new(cdScript)
                {
                    ErrorHandler = ErrorDataReceived,
                    WorkingDirectory = profile.WorkingDirectory
                }).WaitForExitAsync();
            }
        }


        private async Task RestoreFiles(ProgressTask task, ToolProfile profile, ToolProfile sourceProfile, ToolConfiguration config)
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(sourceProfile.ProjectName))
            {
                throw new InvalidOperationException("Unable to load profile name.");
            }

            ContinuousDeploymentConfig cdConfig = new()
            {
                ConfigPath = Path.Combine(config.CDRootPath, sourceProfile.ProjectName, Constants.CD_CONFIG_NAME),
                RepositoryPath = Path.Combine(config.CDRootPath, sourceProfile.ProjectName, Constants.CD_FILES_DIR)
            };

            string originalDescription = task.Description;
            string cdScript = scriptBuilder.SetScript(ScriptType.ContinuousDeploymentRestore).WithPlaceholders(cdConfig).Build();
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


        private async Task StoreFiles(ProgressTask task, ToolProfile profile, ToolConfiguration config)
        {
            if (StopProcessing)
            {
                return;
            }

            if (string.IsNullOrEmpty(profile.ProjectName))
            {
                throw new InvalidOperationException("Unable to load profile name.");
            }

            ContinuousDeploymentConfig cdConfig = new()
            {
                ConfigPath = Path.Combine(config.CDRootPath, profile.ProjectName, Constants.CD_CONFIG_NAME),
                RepositoryPath = Path.Combine(config.CDRootPath, profile.ProjectName, Constants.CD_FILES_DIR)
            };
            string cdScript = scriptBuilder.SetScript(ScriptType.ContinuousDeploymentStore).WithPlaceholders(cdConfig).Build();
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
