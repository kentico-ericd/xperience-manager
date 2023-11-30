using System.Reflection;

public class HelpCommand : Command {
    private static readonly string[] KEYWORDS = new string[] { String.Empty, "help" };
    private static readonly string DESCRIPTION = "Displays the help menu (this screen)";


    public HelpCommand() : base(KEYWORDS, DESCRIPTION)
    {
    }


    public override void Execute()
    {
        var version = Assembly.GetEntryAssembly()?
                                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                    .InformationalVersion
                                    .ToString();

        Console.WriteLine($"xman v{version}");
        Console.WriteLine("-------------");

        foreach (var command in CommandHelper.Commands) {
            if (command is null) continue;

            var keywords = command.Keywords.Where(k => !String.IsNullOrEmpty(k))
                .Select(k => $"--{k}");
            Console.WriteLine($"  {String.Join(", ", keywords)}");
            Console.WriteLine($"    {command.Description}");
        }
    }
}