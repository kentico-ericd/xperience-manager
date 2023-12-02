using Spectre.Console;

using Xperience.Xman.Repositories;

namespace Xperience.Xman
{
    /// <summary>
    /// The main entry point for the console application which supports Dependency Injection.
    /// </summary>
    public class App
    {
        private readonly ICommandRepository commandRepository;


        public App(ICommandRepository commandRepository)
        {
            this.commandRepository = commandRepository;
        }


        /// <summary>
        /// Runs the console application with the arguments provided by the user.
        /// </summary>
        public void Run(string[] args)
        {
            var identifier = "help";
            if (args.Length > 0) {
                identifier = args[0];
            }

            var command = commandRepository.Get(identifier);
            if (command is null)
            {
                return;
            }

            try
            {
                command.Execute(args);
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Process failed with error:\n{e.Message}[/]");
            }

            if (command.Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Process failed with errors:\n{String.Join("\n", command.Errors)}[/]");
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Process complete![/]");
        }
    }
}