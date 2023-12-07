using Spectre.Console;

using Xperience.Xman.Commands;
using Xperience.Xman.Repositories;
using Xperience.Xman.Services;

namespace Xperience.Xman
{
    /// <summary>
    /// The main entry point for the console application which supports Dependency Injection.
    /// </summary>
    public class App
    {
        private readonly IConfigManager configManager;
        private readonly ICommandRepository commandRepository;


        public App(IConfigManager configManager, ICommandRepository commandRepository)
        {
            this.configManager = configManager;
            this.commandRepository = commandRepository;
        }


        /// <summary>
        /// Runs the console application with the arguments provided by the user.
        /// </summary>
        public async Task Run(string[] args)
        {
            await configManager.EnsureConfigFile();

            string identifier = "help";
            if (args.Length > 0)
            {
                identifier = args[0];
            }

            var command = commandRepository.Get(identifier);
            if (command is null)
            {
                return;
            }

            try
            {
                await command.Execute(args);
                if (command.Errors.Any())
                {
                    AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Process failed with errors:\n{string.Join("\n", command.Errors)}[/]");
                }
                else if (command is not HelpCommand and not ProfileCommand)
                {
                    AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Process complete![/]");
                }
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Process failed with error:\n{e.Message}[/]");
            }
        }
    }
}
