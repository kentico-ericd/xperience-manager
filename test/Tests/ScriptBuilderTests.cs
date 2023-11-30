using NUnit.Framework;

using Xperience.Xman.Helpers;
using Xperience.Xman.Models;

namespace Xperience.Xman.Tests
{
    public class ScriptBuilderTests
    {
        [Test]
        public void ProjectInstallScript_WithValidOptions_ReturnsValidScript()
        {
            var options = new InstallOptions();
            var script = new ScriptBuilder(ScriptType.ProjectInstall).WithOptions(options).Build();

            Assert.That(script, Is.EqualTo($"dotnet new {options.Template} -n {options.ProjectName}"));
        }


        [Test]
        public void ProjectInstallScript_WithInvalidOptions_ThrowsException()
        {
            var options = new InstallOptions { Template = String.Empty };
            var builder = new ScriptBuilder(ScriptType.ProjectInstall).WithOptions(options);

            Assert.That(() => builder.Build(), Throws.InvalidOperationException);
        }


        [Test]
        public void DatabaseInstallScript_WithValidOptions_ReturnsValidScript()
        {
            var options = new InstallOptions { ServerName = "TESTSERVER" };
            var script = new ScriptBuilder(ScriptType.DatabaseInstall).WithOptions(options).Build();

            Assert.That(script, Is.EqualTo($"dotnet kentico-xperience-dbmanager -- -s \"{options.ServerName}\" -d \"{options.DatabaseName}\" -a \"{options.AdminPassword}\""));
        }


        [Test]
        public void DatabaseInstallScript_WithInvalidOptions_ThrowsException()
        {
            // Default options has null value
            var options = new InstallOptions();
            var builder = new ScriptBuilder(ScriptType.DatabaseInstall).WithOptions(options);

            Assert.That(() => builder.Build(), Throws.InvalidOperationException);
        }
    }
}