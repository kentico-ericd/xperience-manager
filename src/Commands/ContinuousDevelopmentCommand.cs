using Spectre.Console;

using Xperience.Xman.Configuration;
using Xperience.Xman.Options;
using Xperience.Xman.Services;
using Xperience.Xman.Wizards;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which stores or restores Continuous Integration data.
    /// </summary>
    public class ContinuousDevelopmentCommand : AbstractCommand
    {
        private string? actionName;
        private ToolProfile? profile;
        private const string NEW = "new";
        private readonly IShellRunner shellRunner;
        private readonly IScriptBuilder scriptBuilder;
        private readonly IConfigManager configManager;
        private readonly IWizard<NewCDOptions> wizard;


        public override IEnumerable<string> Keywords => new string[] { "cd" };


        public override IEnumerable<string> Parameters => new string[] { NEW };


        public override string Description => "Creates a new CD configuration or runs CD processes";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal ContinuousDevelopmentCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public ContinuousDevelopmentCommand(IWizard<NewCDOptions> wizard, IShellRunner shellRunner, IScriptBuilder scriptBuilder, IConfigManager configManager)
        {
            this.wizard = wizard;
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

            if (actionName?.Equals(NEW, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var options = await wizard.Run();
                string directory = await MakeConfigDirectory(profile, options);
                string configPath = await CreateConfigFile(profile, directory);
                if (!StopProcessing)
                {
                    await configManager.AddCDProfile(profile, new()
                    {
                        ConfigPath = configPath,
                        EnvironmentName = options.EnvironmentName
                    });
                }
            }
        }


        public override async Task PostExecute(string[] args)
        {
            if (!Errors.Any() && (actionName?.Equals(NEW, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]New CD profile created![/]\n");
            }

            await base.PostExecute(args);
        }


        private async Task<string> CreateConfigFile(ToolProfile profile, string directory)
        {
            if (StopProcessing)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(profile.ProjectName))
            {
                throw new InvalidOperationException("Unable to load project name.");
            }

            string configPath = Path.Combine(directory, "repository.config");
            string projectPath = Path.Combine(Environment.CurrentDirectory, profile.ProjectName, $"{profile.ProjectName}.csproj");
            string mkdirScript = scriptBuilder.SetScript(ScriptType.ContinuousDevelopmentNewConfiguration)
                .AppendPath(configPath)
                .AppendProject(projectPath)
                .Build();
            await shellRunner.Execute(new(mkdirScript) { ErrorHandler = ErrorDataReceived }).WaitForExitAsync();

            return configPath;
        }


        private async Task<string> MakeConfigDirectory(ToolProfile profile, NewCDOptions options)
        {
            if (StopProcessing)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(options.RootConfigPath) || string.IsNullOrEmpty(options.EnvironmentName) || string.IsNullOrEmpty(profile.ProjectName))
            {
                throw new InvalidOperationException("Missing parts to the configuration's full path.");
            }

            string newConfigPath = Path.Combine(options.RootConfigPath, profile.ProjectName, options.EnvironmentName);
            string mkdirScript = scriptBuilder.SetScript(ScriptType.CreateDirectory).AppendDirectory(newConfigPath).Build();
            await shellRunner.Execute(new(mkdirScript) { ErrorHandler = ErrorDataReceived }).WaitForExitAsync();

            return newConfigPath;
        }
    }
}
