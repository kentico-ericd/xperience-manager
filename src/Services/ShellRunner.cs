using System.Diagnostics;

namespace Xperience.Xman.Services
{
    public class ShellRunner : IShellRunner
    {
        public Process Execute(string script, DataReceivedEventHandler? errorHandler = null, DataReceivedEventHandler? outputHandler = null, bool keepOpen = false)
        {
            Process cmd = new();
            cmd.StartInfo.FileName = "powershell.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            if (errorHandler is not null)
            {
                cmd.StartInfo.RedirectStandardError = true;
                cmd.EnableRaisingEvents = true;
                cmd.ErrorDataReceived += errorHandler;
            }

            if (outputHandler is not null)
            {
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.OutputDataReceived += outputHandler;
            }

            cmd.Start();

            if (errorHandler is not null)
            {
                cmd.BeginErrorReadLine();
            }

            if (outputHandler is not null)
            {
                cmd.BeginOutputReadLine();
            }

            cmd.StandardInput.AutoFlush = true;
            cmd.StandardInput.WriteLine(script);
            if (!keepOpen)
            {
                cmd.StandardInput.Close();
            }

            return cmd;
        }
    }
}