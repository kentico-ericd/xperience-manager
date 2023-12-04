using NUnit.Framework;

using Xperience.Xman.Options;
using Xperience.Xman.Services;

namespace Xperience.Xman.Tests
{
    /// <summary>
    /// Tests for <see cref="IScriptBuilder"/>.
    /// </summary>
    public class IScriptBuilderTests
    {
        private IScriptBuilder scriptBuilder = new ScriptBuilder();
        private readonly InstallOptions validInstallOptions = new() { ServerName = "TESTSERVER" };
        private readonly UpdateOptions validUpdateOptions = new() { PackageName = "kentico.xperience.webapp" };


        [Test]
        public void ProjectInstallScript_WithValidOptions_ReturnsValidScript()
        {
            var script = scriptBuilder.SetScript(ScriptType.ProjectInstall).WithOptions(validInstallOptions).Build();
            var expected = $"dotnet new {validInstallOptions.Template} -n {validInstallOptions.ProjectName}";

            Assert.That(script, Is.EqualTo(expected));
        }


        [Test]
        public void ProjectInstallScript_WithInvalidOptions_ThrowsException()
        {
            var options = new InstallOptions { Template = String.Empty };
            var builder = scriptBuilder.SetScript(ScriptType.ProjectInstall).WithOptions(options);

            Assert.That(() => builder.Build(), Throws.InvalidOperationException);
        }


        [Test]
        public void TemplateInstall_AppendVersion_AddsParameter()
        {
            var version = new Version(1, 0, 0);
            var script = scriptBuilder.SetScript(ScriptType.TemplateInstall)
                .WithOptions(validInstallOptions)
                .AppendVersion(version)
                .Build();
            var expected = $"dotnet new install kentico.xperience.templates::{version}";

            Assert.That(script, Is.EqualTo(expected));
        }


        [Test]
        public void PackageUpdate_AppendVersion_AddsParameter()
        {
            var version = new Version(1, 0, 0);
            var script = scriptBuilder.SetScript(ScriptType.PackageUpdate)
                .WithOptions(validUpdateOptions)
                .AppendVersion(version)
                .Build();
            var expected = $"dotnet add package {validUpdateOptions.PackageName} --version {version}";

            Assert.That(script, Is.EqualTo(expected));
        }


        [Test]
        public void DatabaseInstallScript_WithValidOptions_ReturnsValidScript()
        {
            var script = scriptBuilder.SetScript(ScriptType.DatabaseInstall).WithOptions(validInstallOptions).Build();
            var expected = $"dotnet kentico-xperience-dbmanager -- -s \"{validInstallOptions.ServerName}\" -d \"{validInstallOptions.DatabaseName}\" -a \"{validInstallOptions.AdminPassword}\"";
            
            Assert.That(script, Is.EqualTo(expected));
        }


        [Test]
        public void DatabaseInstallScript_WithInvalidOptions_ThrowsException()
        {
            // Default options has null value
            var options = new InstallOptions();
            var builder = scriptBuilder.SetScript(ScriptType.DatabaseInstall).WithOptions(options);

            Assert.That(() => builder.Build(), Throws.InvalidOperationException);
        }
    }
}