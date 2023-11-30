namespace Xperience.Xman.Steps
{
    /// <summary>
    /// Represents a step that requires user input during a command execution.
    /// </summary>
    public class Step
    {
        private readonly bool isReadKey;
        private readonly string prompt;
        private readonly string? errorMessage;
        private readonly Func<string, bool> callback;


        /// <summary>
        /// Initializes a new instance of <see cref="Step"/>.
        /// </summary>
        /// <param name="prompt">The text to show the user when the step begins.</param>
        /// <param name="callback">A function that validates and processes the user input.</param>
        /// <param name="errorMessage">The text to display when the <paramref name="callback"/> returns <c>false</c>.</param>
        /// <param name="isReadKey">If <c>true</c>, <see cref="Console.ReadKey()"/> is used.</param>
        public Step(string prompt, Func<string, bool> callback, string? errorMessage = null, bool isReadKey = false)
        {
            this.prompt = prompt;
            this.errorMessage = errorMessage;
            this.isReadKey = isReadKey;
            this.callback = callback;
        }


        /// <summary>
        /// Displays the step to the user and waits for input.
        /// </summary>
        /// <returns>The user input.</returns>
        public string? Execute()
        {
            XConsole.WriteLine($"\n{prompt}");

            string? input;
            if (isReadKey)
            {
                input = Console.ReadKey().Key.ToString();
            }
            else
            {
                input = Console.ReadLine();
            }

            if (input is not null && !callback.Invoke(input))
            {
                XConsole.WriteErrorLine(errorMessage ?? "There was an error processing the input");
                return Execute();
            }

            return input;
        }
    }
}
