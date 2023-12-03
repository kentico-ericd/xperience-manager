using System.Diagnostics;

namespace Xperience.Xman.Services
{
    public class ShellRunner : IShellRunner
    {
        public Process Execute(string script, DataReceivedEventHandler errorHandler)
        {
            Process cmd = new();
            cmd.StartInfo.FileName = "powershell.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.EnableRaisingEvents = true;
            cmd.ErrorDataReceived += errorHandler;
            cmd.Start();

            cmd.BeginErrorReadLine();
            cmd.BeginOutputReadLine();
            cmd.StandardInput.AutoFlush = true;
            cmd.StandardInput.WriteLine(script);

            return cmd;
        }
    }
}