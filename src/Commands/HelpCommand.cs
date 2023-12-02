using Spectre.Console;

using System.Reflection;

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
            var assembly = Assembly.GetExecutingAssembly();
            var v = assembly.GetName().Version;
            AnsiConsole.Write(
                new FigletText("xman")
                    .LeftJustified()
                    .Color(Color.Orange3));
            if (v is not null) AnsiConsole.WriteLine($" v{v.Major}.{v.Minor}.{v.Revision}\n");
        }
    }
}