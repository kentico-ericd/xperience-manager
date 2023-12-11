using NSubstitute;

using NUnit.Framework;

using Xperience.Xman.Commands;
using Xperience.Xman.Configuration;
using Xperience.Xman.Services;

namespace Xperience.Xman.Tests.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="ContinuousIntegrationCommand"/>.
    /// </summary>
    public class ContinuousIntegrationCommandTests : TestBase
    {
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
        private readonly IConfigManager configManager = Substitute.For<IConfigManager>();


        [SetUp]
        public void ContinuousIntegrationCommandTestsSetUp()
        {
            configManager.GetCurrentProfile().Returns(new ToolProfile());
            shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) => GetDummyProcess());
        }


        [Test]
        public async Task Execute_StoreParameter_CallsStoreScript()
        {
            string[] args = new string[] { "ci", "store" };
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder(), configManager);
            await command.PreExecute(args);
            await command.Execute(args);

            string expectedScript = "dotnet run --no-build --kxp-ci-store";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedScript)));
        }


        [Test]
        public async Task Execute_RestoreParameter_CallsRestoreScript()
        {
            string[] args = new string[] { "ci", "restore" };
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder(), configManager);
            await command.PreExecute(args);
            await command.Execute(args);

            string expectedScript = "dotnet run --no-build --kxp-ci-restore";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedScript)));
        }
    }
}
