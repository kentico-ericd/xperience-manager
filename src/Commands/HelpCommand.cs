using Spectre.Console;

using System.Reflection;

using Xperience.Xman.Helpers;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which displays the current tool version, a list of commands, and their descriptions.
    /// </summary>
    public class HelpCommand : AbstractCommand
    {
        public override IEnumerable<string> Keywords => new string[] { "?", "help" };


        public override IEnumerable<string> Parameters => Array.Empty<string>();


        public override string Description => "Displays the help menu (this screen)";


        public override void Execute(string[] args)
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            AnsiConsole.Write(
                new FigletText("xman")
                    .LeftJustified()
                    .Color(Color.Orange3));
            if (v is not null) AnsiConsole.WriteLine($" v{v.Major}.{v.Minor}.{v.Revision}");

            var table = new Table()
                .AddColumn("Command")
                .AddColumn("Parameters")
                .AddColumn("Description");
            foreach (var command in CommandHelper.Commands)
            {
                if (command is null) continue;

                table.AddRow(
                    String.Join(", ", command.Keywords),
                    String.Join(", ", command.Parameters),
                    command.Description);
            }

            AnsiConsole.Write(table);
        }
    }
}