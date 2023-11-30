using System.Diagnostics;

using Xperience.Xman.Commands;

namespace Xperience.Xman.Helpers
{
    /// <summary>
    /// Contains methods to assist with <see cref="ICommand"/>s and scripts.
    /// </summary>
    public static class CommandHelper
    {
        private static IEnumerable<ICommand?>? commands;


        /// <summary>
        /// A list of all registered commands.
        /// </summary>
        public static IEnumerable<ICommand?> Commands
        {
            get
            {
                commands ??= AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .Where(c => typeof(ICommand).IsAssignableFrom(c) && !c.IsInterface)
                        .Select(c => Activator.CreateInstance(c) as ICommand);

                return commands;
            }
        }


        /// <summary>
        /// Gets the command whose <see cref="ICommand.Keywords"/> matches the first argument passed.
        /// </summary>
        /// <param name="args">The arguments passed from the CLI.</param>
        /// <returns>The requested command, or <c>null</c>.</returns>
        public static ICommand? GetCommand(string[] args)
        {
            if (!args.Any())
            {
                return new HelpCommand();
            }

            var commandArg = args[0];

            return Commands.FirstOrDefault(c => c?.Keywords.Contains(commandArg, StringComparer.OrdinalIgnoreCase) ?? false);
        }


        /// <summary>
        /// Executes a script in a hidden shell with redirected streams.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        /// <param name="keepOpen">If <c>true</c>, the <see cref="Process.StandardInput"/> is left open after the script executes to
        /// allow further input.</param>
        /// <returns>The script process.</returns>
        public static Process ExecuteShell(string script, bool keepOpen = false)
        {
            Process cmd = new();
            cmd.StartInfo.FileName = "powershell.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.EnableRaisingEvents = true;
            cmd.Start();

            cmd.StandardInput.AutoFlush = true;
            cmd.StandardInput.WriteLine(script);
            if (!keepOpen)
            {
                cmd.StandardInput.Close();
            }

            return cmd;
        }
    }
}