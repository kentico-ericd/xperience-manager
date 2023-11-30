namespace Xperience.Xman
{
    /// <summary>
    /// A wrapper for <see cref="Console"/> which simplifies message formatting.
    /// </summary>
    public static class XConsole
    {
        private const ConsoleColor defaultForeground = ConsoleColor.White;
        private const ConsoleColor successForeground = ConsoleColor.Green;
        private const ConsoleColor errorForeground = ConsoleColor.Red;
        private const ConsoleColor emphasisForeground = ConsoleColor.Blue;


        public static void WriteLine(string input, ConsoleColor color = defaultForeground)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(input);
            Console.ResetColor();
        }


        public static void WriteSuccessLine(string input)
        {
            Console.ForegroundColor = successForeground;
            Console.WriteLine(input);
            Console.ResetColor();
        }


        public static void WriteErrorLine(string input)
        {
            Console.ForegroundColor = errorForeground;
            Console.WriteLine(input);
            Console.ResetColor();
        }


        public static void WriteEmphasisLine(string input)
        {
            Console.ForegroundColor = emphasisForeground;
            Console.WriteLine(input);
            Console.ResetColor();
        }
    }
}
