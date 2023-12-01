using Xperience.Xman.Helpers;

namespace Xperience.Xman
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var command = CommandHelper.GetCommand(args);
            command?.Execute();
        }
    }
}