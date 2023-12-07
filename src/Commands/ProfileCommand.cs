using Spectre.Console;

using Xperience.Xman.Services;

namespace Xperience.Xman.Commands
{
    public class ProfileCommand : AbstractCommand
    {
        private readonly IConfigManager configManager;


        public override IEnumerable<string> Keywords => new string[] { "p", "profile" };


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Switches the current profile";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
        internal ProfileCommand()
        {
        }


        public ProfileCommand(IConfigManager configManager) => this.configManager = configManager;


        public override async Task Execute(string[] args)
        {
            var config = await configManager.GetConfig();
            if (!config.Profiles.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"There are no registered profiles. Install a new instance with [{Constants.SUCCESS_COLOR}]xman i[/] to add a profile.\n");
                return;
            }

            var currentProfile = await configManager.GetCurrentProfile();
            PrintCurrentProfile(currentProfile);

            var prompt = new SelectionPrompt<string>()
                    .Title("Switch to profile:")
                    .PageSize(10)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(config.Profiles.Select(p => p.ProjectName ?? string.Empty));

            string selected = AnsiConsole.Prompt(prompt);
            if (await configManager.TrySetCurrentProfile(selected))
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Switched to '{selected}'[/]");
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Failed to switch to '{selected}'[/]");
            }

            AnsiConsole.WriteLine();
        }
    }
}
