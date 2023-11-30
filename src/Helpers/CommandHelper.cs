using System.Diagnostics;

using Xperience.Xman.Commands;

namespace Xperience.Xman.Helpers
{
    public static class CommandHelper
    {
        private static IEnumerable<ICommand?>? commands;


        public static IEnumerable<ICommand?> Commands
        {
            get
            {
                if (commands is null)
                {
                    commands = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .Where(c => typeof(ICommand).IsAssignableFrom(c) && !c.IsInterface)
                        .Select(c => Activator.CreateInstance(c) as ICommand);
                }

                return commands;
            }
        }


        public static ICommand? GetCommand(string[] args)
        {
            if (!args.Any())
            {
                return new HelpCommand();
            }

            var commandArg = args[0];

            return Commands.FirstOrDefault(c => c?.Keywords.Contains(commandArg, StringComparer.OrdinalIgnoreCase) ?? false);
        }


        public static Process ExecuteShell(string command, bool keepOpen = false)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "powershell.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.EnableRaisingEvents = true;
            cmd.Start();

            cmd.StandardInput.AutoFlush = true;
            cmd.StandardInput.WriteLine(command);
            if (!keepOpen)
            {
                cmd.StandardInput.Close();
            }

            return cmd;
        }
    }
}