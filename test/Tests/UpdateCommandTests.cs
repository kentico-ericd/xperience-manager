using NSubstitute;

using NUnit.Framework;

using System.Diagnostics;

using Xperience.Xman.Commands;
using Xperience.Xman.Options;
using Xperience.Xman.Services;
using Xperience.Xman.Wizards;

namespace Xperience.Xman.Tests
{
    /// <summary>
    /// Tests for <see cref="UpdateCommand"/>.
    /// </summary>
    public class UpdateCommandTests
    {
        private readonly Version version = new(1, 0, 0);
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
        private readonly IWizard<UpdateOptions> updateWizard = Substitute.For<IWizard<UpdateOptions>>();


        [SetUp]
        public void UpdateCommandTestsSetUp()
        {
            updateWizard.Run().Returns(new UpdateOptions
            {
                Version = version
            });

            shellRunner
                .Execute(Arg.Any<ShellOptions>())
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
        public async Task Execute_CallsUpdateScripts()
        {
            var command = new UpdateCommand(shellRunner, new ScriptBuilder(), updateWizard);
            await command.Execute(Array.Empty<string>());

            string[] packageNames = new string[]
            {
                "kentico.xperience.admin",
                "kentico.xperience.azurestorage",
                "kentico.xperience.cloud",
                "kentico.xperience.graphql",
                "kentico.xperience.imageprocessing",
                "kentico.xperience.webapp"
            };

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals("dotnet build")));
            foreach (string p in packageNames)
            {
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals($"dotnet add package {p} --version {version}")));
            }
        }
    }
}
