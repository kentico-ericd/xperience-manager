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
    /// Tests for <see cref="InstallCommand"/>.
    /// </summary>
    public class InstallCommandTests
    {
        private const string dbName = "TESTDB";
        private const string password = "PW";
        private const string serverName = "TESTSERVER";
        private const string template = "TEMPLATE";
        private const string projectName = "PROJECT";
        private readonly Version version = new Version(1, 0, 0);
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
        private readonly IWizard<InstallOptions> installWizard = Substitute.For<IWizard<InstallOptions>>();
        

        [SetUp]
        public void InstallCommandTestsSetUp()
        {
            installWizard.Run().Returns(new InstallOptions
            {
                ProjectName = projectName,
                AdminPassword = password,
                DatabaseName = dbName,
                ServerName = serverName,
                Version = version,
                Template = template
            });

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


        [TearDown]
        public void InstallCommandTestsTearDown()
        {
            File.Delete(Constants.CONFIG_FILENAME);
        }


        [Test]
        public async Task Execute_CallsInstallationScripts()
        {
            var command = new InstallCommand(shellRunner, new ScriptBuilder(), installWizard);
            await command.Execute(Array.Empty<string>());

            var expectedProjectFileScript = $"dotnet new {template} -n {projectName}";
            var expectedUninstallScript = "dotnet new uninstall kentico.xperience.templates";
            var expectedTemplateScript = $"dotnet new install kentico.xperience.templates::{version}";
            var expectedDatabaseScript = $"dotnet kentico-xperience-dbmanager -- -s \"{serverName}\" -d \"{dbName}\" -a \"{password}\"";

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Constants.CONFIG_FILENAME));
                shellRunner.Received().Execute(expectedProjectFileScript, Arg.Any<DataReceivedEventHandler>(), Arg.Any<DataReceivedEventHandler>(), Arg.Any<bool>());
                shellRunner.Received().Execute(expectedUninstallScript, Arg.Any<DataReceivedEventHandler>(), Arg.Any<DataReceivedEventHandler>(), Arg.Any<bool>());
                shellRunner.Received().Execute(expectedTemplateScript, Arg.Any<DataReceivedEventHandler>(), Arg.Any<DataReceivedEventHandler>(), Arg.Any<bool>());
                shellRunner.Received().Execute(expectedDatabaseScript, Arg.Any<DataReceivedEventHandler>(), Arg.Any<DataReceivedEventHandler>(), Arg.Any<bool>());
            });
        }
    }
}
