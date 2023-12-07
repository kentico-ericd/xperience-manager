using Spectre.Console;

using Xperience.Xman.Helpers;
using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="InstallOptions"/> for installing Xperience by Kentico.
    /// </summary>
    public class InstallWizard : AbstractWizard<InstallOptions>
    {
        private readonly IEnumerable<string> templates = new string[] {
            "kentico-xperience-sample-mvc",
            "kentico-xperience-mvc",
            "kentico-xperience-admin-sample"
        };


        public override void InitSteps()
        {
            var versions = NuGetVersionHelper.GetPackageVersions("kentico.xperience.templates")
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult()
                .Where(v => !v.IsPrerelease && !v.IsLegacyVersion && v.Major >= 25)
                .Select(v => v.Version)
                .OrderByDescending(v => v);
            Steps.Add(new Step<Version>(
                new SelectionPrompt<Version>()
                    .Title("Which [green]version[/]?")
                    .PageSize(10)
                    .UseConverter(v => $"{v.Major}.{v.Minor}.{v.Build}")
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(versions),
                (v) => Options.Version = v));

            Steps.Add(new Step<string>(
                new SelectionPrompt<string>()
                    .Title("Which [green]template[/]?")
                    .AddChoices(templates),
                (v) => Options.Template = v.ToString()));

            Steps.Add(new Step<string>(
                new TextPrompt<string>("Give your project a [green]name[/]:")
                    .DefaultValue(Options.ProjectName),
                (v) => Options.ProjectName = v));

            var cloudPrompt = new ConfirmationPrompt("Prepare for [green]cloud[/] deployment?")
            {
                DefaultValue = Options.UseCloud
            };
            Steps.Add(new Step<bool>(cloudPrompt, (v) => Options.UseCloud = v));

            Steps.Add(new Step<string>(
                new TextPrompt<string>("Enter the [green]SQL server[/] name:"),
                (v) => Options.ServerName = v));

            Steps.Add(new Step<string>(
                new TextPrompt<string>("Enter the [green]database[/] name:")
                    .AllowEmpty()
                    .DefaultValue(Options.DatabaseName),
                (v) => Options.DatabaseName = v));

            Steps.Add(new Step<string>(
                new TextPrompt<string>("Enter the admin [green]password[/]:")
                    .AllowEmpty()
                    .DefaultValue(Options.AdminPassword),
                (v) => Options.AdminPassword = v));
        }
    }
}
