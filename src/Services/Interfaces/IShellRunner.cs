using System.Diagnostics;

namespace Xperience.Xman.Services
{
    /// <summary>
    /// Contains methods for executing shell scripts.
    /// </summary>
    public interface IShellRunner : IService
    {
        /// <summary>
        /// Executes a script in a hidden shell with redirected streams.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        /// <param name="errorHandler">The handler called when the script encounters an error.</param>
        /// <param name="outputHandler">The handler called when the script outputs data.</param>
        /// <param name="keepOpen">If <c>true</c>, the <see cref="Process.StandardInput"/> is not closed.</param>
        /// <returns>The script process.</returns>
        public Process Execute(string script, DataReceivedEventHandler? errorHandler = null, DataReceivedEventHandler? outputHandler = null, bool keepOpen = false);
    }
}