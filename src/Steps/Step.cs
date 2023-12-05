using Spectre.Console;

namespace Xperience.Xman.Steps
{
    /// <summary>
    /// A step used to display a prompt for user interaction and optionally return the value.
    /// </summary>
    public class Step<T> : IStep
    {
        private readonly IPrompt<T> prompt;
        private readonly Action<T>? valueReceiver;


        /// <summary>
        /// Initializes a new instance of <see cref="Step{T}"/>.
        /// </summary>
        /// <param name="prompt">The prompt to show the user when the step begins.</param>
        /// <param name="valueReceiver">A function that is passed the user input.</param>
        public Step(IPrompt<T> prompt, Action<T>? valueReceiver = null)
        {
            this.prompt = prompt;
            this.valueReceiver = valueReceiver;
        }


        public void Execute()
        {
            var input = AnsiConsole.Prompt(prompt);
            if (valueReceiver is not null)
            {
                valueReceiver(input);
            }
        }
    }
}
