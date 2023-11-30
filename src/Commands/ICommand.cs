namespace Xperience.Xman.Commands
{
    /// <summary>
    /// Describes an executable command from the CLI.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The parameters in the first position that will trigger this command.
        /// </summary>
        public IEnumerable<string> Keywords { get; }


        /// <summary>
        /// A description to display in the Help menu.
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// Executes the command.
        /// </summary>
        public void Execute();
    }
}