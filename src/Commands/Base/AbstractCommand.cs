using System.Diagnostics;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// Describes an executable command from the CLI.
    /// </summary>
    public abstract class AbstractCommand : ICommand
    {
        private readonly List<string> errors = new();


        public List<string> Errors => errors;


        public bool StopProcessing { get; set; }

        
        public abstract IEnumerable<string> Keywords { get; }


        public abstract IEnumerable<string> Parameters { get; }


        public abstract string Description { get; }


        public abstract Task Execute(string[] args);


        /// <summary>
        /// A handler which can be assigned to <see cref="Process.ErrorDataReceived"/> to handler errors. 
        /// </summary>
        protected void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                LogError(e.Data, sender as Process);
            }
        }


        /// <summary>
        /// Adds an error to <see cref="Errors"/> and stops additional processing.
        /// </summary>
        protected void LogError(string message, Process? process)
        {
            Errors.Add(message);

            StopProcessing = true;
            if (process is not null && !process.HasExited)
            {
                process.Kill();
            }
        }
    }
}