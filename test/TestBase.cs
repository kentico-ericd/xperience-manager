using System.Diagnostics;

namespace Xperience.Xman.Tests
{
    public class TestBase
    {
        /// <summary>
        /// Gets a <see cref="Process"/> which does nothing.
        /// </summary>
        protected Process GetDummyProcess()
        {
            Process cmd = new();
            cmd.StartInfo.FileName = "powershell.exe";
            cmd.StartInfo.Arguments = "-noprofile -nologo";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.AutoFlush = true;
            cmd.StandardInput.WriteLine("dotnet --version");
            cmd.StandardInput.Close();

            return cmd;
        }
    }
}