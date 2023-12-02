using Spectre.Console;

using Xperience.Xman.Helpers;

namespace Xperience.Xman
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var command = CommandHelper.GetCommand(args);
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