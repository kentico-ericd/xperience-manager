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
        /// Runs before <see cref="Execute"/>.
        /// </summary>
        public Task PreExecute(string[] args);


        /// <summary>
        /// Executes the command if <see cref="StopProcessing"/> is <c>false</c>.
        /// </summary>
        /// <param name="args">The arguments provided by the user.</param>
        public Task Execute(string[] args);


        /// <summary>
        /// Runs after <see cref="Execute"/> if <see cref="StopProcessing"/> is <c>false</c>.
        /// </summary>
        public Task PostExecute(string[] args);
    }
}
