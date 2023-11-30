using System.Reflection;

using Xperience.Xman.Helpers;

namespace Xperience.Xman.Commands
{
    public class HelpCommand : ICommand
    {
        public IEnumerable<string> Keywords => new string[] { "help" };


        public string Description => "Displays the help menu (this screen)";


        public void Execute()
        {
            var version = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                .ToString();

            Console.WriteLine($"xman v{version}");
            Console.WriteLine("-------------");

            foreach (var command in CommandHelper.Commands)
            {
                if (command is null) continue;

                var keywords = command.Keywords.Where(k => !string.IsNullOrEmpty(k));
                Console.WriteLine($"\n{string.Join(", ", keywords)}");
                Console.WriteLine($"\t-{command.Description}");
            }
        }
    }
}