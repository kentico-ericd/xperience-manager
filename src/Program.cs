public class Program
{
    static void Main(string[] args)
    {
        var command = CommandHelper.GetCommand(args);
        if (command is not null) {
            command.Execute();
        }
    }
}