using NSubstitute;

using NUnit.Framework;

using Xperience.Xman.Commands;
using Xperience.Xman.Services;

namespace Xperience.Xman.Tests.Tests.Commands
{
    /// <summary>
    /// Tests for <see cref="ContinuousIntegrationCommand"/>.
    /// </summary>
    public class ContinuousIntegrationCommandTests : TestBase
    {
        private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();


        [SetUp]
        public void ContinuousIntegrationCommandTestsSetUp() => shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) => GetDummyProcess());


        [Test]
        public async Task Execute_StoreParameter_CallsStoreScript()
        {
            string[] args = new string[] { "ci", "store" };
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder());
            await command.PreExecute(new(), args);
            await command.Execute(new(), args);

            string expectedScript = "dotnet run --no-build --kxp-ci-store";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedScript)));
        }


        [Test]
        public async Task Execute_RestoreParameter_CallsRestoreScript()
        {
            string[] args = new string[] { "ci", "restore" };
            var command = new ContinuousIntegrationCommand(shellRunner, new ScriptBuilder());
            await command.PreExecute(new(), args);
            await command.Execute(new(), args);

            string expectedScript = "dotnet run --no-build --kxp-ci-restore";

            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(expectedScript)));
        }
    }
}
