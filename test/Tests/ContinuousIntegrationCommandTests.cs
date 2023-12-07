using NSubstitute;

using NUnit.Framework;

using System.Diagnostics;

using Xperience.Xman.Commands;
using Xperience.Xman.Services;

namespace Xperience.Xman.Tests
{
    /// <summary>
    /// Tests for <see cref="ContinuousIntegrationCommand"/>.
    /// </summary>
    public class ContinuousIntegrationCommandTests
    {
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();


        [SetUp]
        public void ContinuousIntegrationCommandTestsSetUp()
        {
            shellRunner
                .Execute(Arg.Any<string>(), Arg.Any<DataReceivedEventHandler>(), Arg.Any<DataReceivedEventHandler>(), Arg.Any<bool>())
                .Returns((x) =>
                {
                    // Return dummy process
                    Process cmd = new();
                    cmd.StartInfo.FileName = "powershell.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();

                    cmd.StandardInput.AutoFlush = true;
                    cmd.StandardInput.WriteLine("dotnet --version");
                    cmd.StandardInput.Close();

                    return cmd;
                });
        }


        [Test]
        public async Task Execute_StoreParameter_CallsStoreScript()
        {
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder());
            await command.Execute(new string[] { "ci", "store" });

            shellRunner.Received().Execute("dotnet run --no-build --kxp-ci-store", Arg.Any<DataReceivedEventHandler>(), Arg.Any<DataReceivedEventHandler>(), Arg.Any<bool>());
        }


        [Test]
        public async Task Execute_RestoreParameter_CallsRestoreScript()
        {
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder());
            await command.Execute(new string[] { "ci", "restore" });

            shellRunner.Received().Execute("dotnet run --no-build --kxp-ci-restore", Arg.Any<DataReceivedEventHandler>(), Arg.Any<DataReceivedEventHandler>(), Arg.Any<bool>());
        }
    }
}
