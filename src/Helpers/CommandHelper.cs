using System.Diagnostics;

public static class CommandHelper
{
    private static IEnumerable<Command?>? commands;


    public static IEnumerable<Command?> Commands
    {
        get
        {
            if (commands is null) {
                commands = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsSubclassOf(typeof(Command)))
                    .Select(type => Activator.CreateInstance(type) as Command);
            }

            return commands;
        }
    }


    public static Command? GetCommand(string[] args) {
        if (!args.Any()) {
            return new HelpCommand();
        }

        var commandArg = args[0];

        return Commands.FirstOrDefault(c => c?.Keywords.Contains(commandArg, StringComparer.OrdinalIgnoreCase) ?? false);
    }


    public static Process ExecuteShell(string command, bool keepOpen = false) {
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