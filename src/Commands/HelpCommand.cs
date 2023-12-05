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
            AnsiConsole.Write(
                new FigletText("xman")
                    .LeftJustified()
                    .Color(Color.Orange3));

            var assembly = Assembly.GetExecutingAssembly();
            var v = assembly.GetName().Version;
            if (v is not null)
            {
                AnsiConsole.WriteLine($" v{v.Major}.{v.Minor}.{v.Revision}");

                var latestVersion = NuGetVersionHelper.GetLatestVersion("xperience.xman", v).ConfigureAwait(false).GetAwaiter().GetResult();
                if (latestVersion is not null)
                {
                    AnsiConsole.MarkupInterpolated($" New version [{Constants.SUCCESS_COLOR}]{latestVersion}[/] available!\n");
                }
            }

            AnsiConsole.MarkupInterpolated($" [{Constants.EMPHASIS_COLOR}]https://github.com/kentico-ericd/xperience-manager[/]\n\n");
        }
    }
}