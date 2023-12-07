using NUnit.Framework;

using System.Diagnostics;
using System.Text;

using Xperience.Xman.Services;

namespace Xperience.Xman.Tests
{
    /// <summary>
    /// Tests for <see cref="IShellRunner"/>.
    /// </summary>
    public class IShellRunnerTests
    {
        private readonly IShellRunner shellRunner = new ShellRunner();


        [Test]
        public void Execute_ErrorHandler_CapturesError()
        {
            var builder = new StringBuilder();
            var invalidPackage = "PACKAGE_DOESNT_EXIST";
            var proc = shellRunner.Execute($"dotnet new install {invalidPackage}", errorHandler: (o, e) => builder.Append(e.Data));
            proc.WaitForExit();

            Assert.Multiple(() =>
            {
                Assert.That(proc.HasExited);
                Assert.That(proc.StartInfo.RedirectStandardError);
                Assert.That(builder.ToString(), Contains.Substring($"{invalidPackage} could not be installed, the package does not exist"));
            });
        }


        [Test]
        public void Execute_OutputHandler_ExitsProcess()
        {
            var question = "How old";
            var proc = shellRunner.Execute($"Read-Host '{question}'", outputHandler: (o, e) =>
            {
                var p = o as Process;
                if (e.Data?.Contains(question) ?? false)
                {
                    p?.StandardInput.WriteLine(42);
                    p?.StandardInput.Close();
                }
            }, keepOpen: true);
            proc.WaitForExit();

            Assert.Multiple(() =>
            {
                Assert.That(proc.HasExited);
                Assert.That(proc.StartInfo.RedirectStandardOutput);
            });
        }
    }
}