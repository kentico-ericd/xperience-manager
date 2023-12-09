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
        private const string DB_NAME = "TESTDB";
        private const string PASSWORD = "PW";
        private const string SERVER_NAME = "TESTSERVER";
        private const string TEMPLATE = "TEMPLATE";
        private const string PROJECT_NAME = "PROJECT";
        private readonly Version version = new(1, 0, 0);
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
        private readonly IWizard<InstallOptions> installWizard = Substitute.For<IWizard<InstallOptions>>();


        [SetUp]
        public void InstallCommandTestsSetUp()
        {
            installWizard.Run().Returns(new InstallOptions
            {
                ProjectName = PROJECT_NAME,
                AdminPassword = PASSWORD,
                DatabaseName = DB_NAME,
                ServerName = SERVER_NAME,
                Version = version,
                Template = TEMPLATE
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
        public async Task Execute_CallsInstallationScripts()
        {
            var command = new InstallCommand(shellRunner, new ScriptBuilder(), installWizard, Substitute.For<IConfigManager>());
            await command.PreExecute(Array.Empty<string>());
            await command.Execute(Array.Empty<string>());

            string expectedProjectFileScript = $"dotnet new {TEMPLATE} -n {PROJECT_NAME}";
            string expectedUninstallScript = "dotnet new uninstall kentico.xperience.templates";
            string expectedTemplateScript = $"dotnet new install kentico.xperience.templates::{version}";
            string expectedDatabaseScript = $"dotnet kentico-xperience-dbmanager -- -s \"{SERVER_NAME}\" -d \"{DB_NAME}\" -a \"{PASSWORD}\"";

            Assert.Multiple(() =>
            {
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedProjectFileScript)));
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedUninstallScript)));
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedTemplateScript)));
                shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedDatabaseScript)));
            });
        }
    }
}
