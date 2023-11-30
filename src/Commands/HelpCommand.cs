using System.Reflection;

using Xperience.Xman.Helpers;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which displays the current tool version, a list of commands, and their descriptions.
    /// </summary>
    public class HelpCommand : ICommand
    {
        public IEnumerable<string> Keywords => new string[] { "?", "help" };


        public string Description => "Displays the help menu (this screen)";


        public void Execute()
        {
            var version = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                .ToString();

            XConsole.WriteLine($"xman v{version}");
            XConsole.WriteLine("-------------");

            foreach (var command in CommandHelper.Commands)
            {
                if (command is null) continue;

                var keywords = command.Keywords.Where(k => !string.IsNullOrEmpty(k));
                XConsole.WriteEmphasisLine($"\n{string.Join(", ", keywords)}");
                XConsole.WriteLine($"\t-{command.Description}");
            }
        }
    }
}