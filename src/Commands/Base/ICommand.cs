namespace Xperience.Xman.Commands
{
    /// <summary>
    /// Describes an executable command from the CLI.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// If <c>true</c>, execution should stop.
        /// </summary>
        public bool StopProcessing { get; set; }


        /// <summary>
        /// A list of errors encountered during execution.
        /// </summary>
        public List<string> Errors { get; }


        /// <summary>
        /// The parameters in the first position that will trigger this command.
        /// </summary>
        public IEnumerable<string> Keywords { get; }


        /// <summary>
        /// The allowed parameters for single-parameter commands.
        /// </summary>
        public IEnumerable<string> Parameters { get; }


        /// <summary>
        /// A description to display in the Help menu.
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="args">The arguments provided by the user.</param>
        public void Execute(string[] args);
    }
}