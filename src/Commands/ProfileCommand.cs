using Spectre.Console;

using Xperience.Xman.Configuration;
using Xperience.Xman.Services;

namespace Xperience.Xman.Commands
{
    public class ProfileCommand : AbstractCommand
    {
        private string? actionName;
        private const string ADD = "add";
        private const string DELETE = "delete";
        private const string SWITCH = "switch";
        private readonly IConfigManager configManager;


        public override IEnumerable<string> Keywords => new string[] { "p", "profile" };


        public override IEnumerable<string> Parameters => new string[] { ADD, DELETE, SWITCH };


        public override string Description => "Manage and switch installation profiles";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal ProfileCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public ProfileCommand(IConfigManager configManager) => this.configManager = configManager;


        public override async Task PreExecute(string[] args)
        {
            actionName = args.Length < 2 ? SWITCH : args[1];
            if (!Parameters.Any(p => p.Equals(actionName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Invalid parameter '{actionName}'");
            }

            var config = await configManager.GetConfig();

            // Can't switch or delete if there are no profiles
            if (!config.Profiles.Any() &&
                (actionName.Equals(SWITCH, StringComparison.OrdinalIgnoreCase) || actionName.Equals(DELETE, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLineInterpolated($"There are no registered profiles. Install a new instance with [{Constants.SUCCESS_COLOR}]xman i[/] to add a profile.\n");
                StopProcessing = true;
                return;
            }

            var profile = await configManager.GetCurrentProfile();
            PrintCurrentProfile(profile);

            if (config.Profiles.Count == 1 && actionName.Equals(SWITCH, StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.WriteLine("You're currently using the only registered profile.\n");
                StopProcessing = true;
            }
        }


        public override async Task Execute(string[] args)
        {
            var config = await configManager.GetConfig();
            if (actionName?.Equals(SWITCH, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await SwitchProfile(config.Profiles);
            }
            else if (actionName?.Equals(ADD, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await AddProfile();
            }
            else if (actionName?.Equals(DELETE, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                await DeleteProfile(config.Profiles);
            }
        }


        public override async Task PostExecute(string[] args)
        {
            // Add some padding after messages
            AnsiConsole.WriteLine();

            await base.PostExecute(args);
        }


        private async Task AddProfile()
        {
            string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the [green]name[/] of the subfolder containing your Xperience project:"));
            string fullPath = Path.GetFullPath(name);
            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"The directory {fullPath} couldn't be found.");
            }

            await configManager.AddProfile(new()
            {
                ProjectName = name,
                WorkingDirectory = fullPath
            });

            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Profile '{name}' added[/]");
        }


        private async Task DeleteProfile(List<ToolProfile> profiles)
        {
            var profile = AnsiConsole.Prompt(new SelectionPrompt<ToolProfile>()
                .Title("Delete which [green]profile[/]?")
                .PageSize(10)
                .UseConverter(p => p.ProjectName ?? string.Empty)
                .MoreChoicesText("Scroll for more...")
                .AddChoices(profiles));

            await configManager.RemoveProfile(profile);

            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Profile '{profile.ProjectName}' deleted[/]");
        }


        private async Task SwitchProfile(List<ToolProfile> profiles)
        {
            var prompt = new SelectionPrompt<ToolProfile>()
                    .Title("Switch to profile:")
                    .PageSize(10)
                    .UseConverter(p => p.ProjectName ?? string.Empty)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(profiles);

            var profile = AnsiConsole.Prompt(prompt);
            await configManager.SetCurrentProfile(profile);

            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Switched to '{profile.ProjectName}'[/]");
        }
    }
}
