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

            command.Execute(args);
            if (command.Errors.Any())
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.ERROR_COLOR}]Process failed with errors:\n{String.Join("\n", command.Errors)}[/]");
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Process complete![/]");
            }
        }
    }
}