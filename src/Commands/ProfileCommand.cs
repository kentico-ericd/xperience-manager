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
        internal ProfileCommand()
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
                await AddProfile(config.Profiles);
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


        private async Task AddProfile(List<ToolProfile> profiles)
        {
            string name = AnsiConsole.Prompt(new TextPrompt<string>("Enter the [green]name[/] of the subfolder containing your Xperience project:"));
            string fullPath = Path.GetFullPath(name);
            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"The directory {fullPath} couldn't be found.");
            }

            if (profiles.Any(p => p.ProjectName?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]There is already a profile named '{name}'[/]");
                return;
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
            string name = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Delete which [green]profile[/]?")
                .PageSize(10)
                .MoreChoicesText("Scroll for more...")
                .AddChoices(profiles.Select(p => p.ProjectName ?? string.Empty)));

            var match = profiles.FirstOrDefault(p => p.ProjectName?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false);
            if (string.IsNullOrEmpty(name) || match is null)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]No matching profile found[/]");
                return;
            }

            await configManager.RemoveProfile(name);

            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Profile '{name}' deleted[/]");
        }


        private async Task SwitchProfile(List<ToolProfile> profiles)
        {
            var prompt = new SelectionPrompt<string>()
                    .Title("Switch to profile:")
                    .PageSize(10)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(profiles.Select(p => p.ProjectName ?? string.Empty));

            string selected = AnsiConsole.Prompt(prompt);
            if (await configManager.TrySetCurrentProfile(selected))
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Switched to '{selected}'[/]");
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Failed to switch to '{selected}'[/]");
            }
        }
    }
}
