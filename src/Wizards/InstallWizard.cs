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
            Constants.TEMPLATE_SAMPLE,
            Constants.TEMPLATE_BLANK,
            Constants.TEMPLATE_ADMIN
        };


        public override async Task InitSteps()
        {
            var versions = await NuGetVersionHelper.GetPackageVersions(Constants.TEMPLATES_PACKAGE);
            var filtered = versions.Where(v => !v.IsPrerelease && !v.IsLegacyVersion && v.Major >= 25)
                .Select(v => v.Version)
                .OrderByDescending(v => v);

            Steps.Add(new Step<Version>(new()
            {
                Prompt = new SelectionPrompt<Version>()
                    .Title("Which [green]version[/]?")
                    .PageSize(10)
                    .UseConverter(v => $"{v.Major}.{v.Minor}.{v.Build}")
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(filtered),
                ValueReceiver = (v) => Options.Version = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new SelectionPrompt<string>()
                    .Title("Which [green]template[/]?")
                    .AddChoices(templates),
                ValueReceiver = (v) => Options.Template = v.ToString()
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>("Give your project a [green]name[/]:")
                    .DefaultValue(Options.ProjectName),
                ValueReceiver = (v) => Options.ProjectName = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>("Install [green]where[/]?")
                    .DefaultValue(Options.InstallRootPath),
                ValueReceiver = (v) => Options.InstallRootPath = v
            }));

            var cloudPrompt = new ConfirmationPrompt("Prepare for [green]cloud[/] deployment?")
            {
                DefaultValue = Options.UseCloud
            };
            Steps.Add(new Step<bool>(new()
            {
                Prompt = cloudPrompt,
                ValueReceiver = (v) => Options.UseCloud = v,
                SkipChecker = IsAdminTemplate
            }));


            var serverPrompt = new TextPrompt<string>("Enter the [green]SQL server[/] name:");
            if (!string.IsNullOrEmpty(Options.ServerName))
            {
                serverPrompt.DefaultValue(Options.ServerName);
            }
            Steps.Add(new Step<string>(new()
            {
                Prompt = serverPrompt,
                ValueReceiver = (v) => Options.ServerName = v,
                SkipChecker = IsAdminTemplate
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>("Enter the [green]database[/] name:")
                    .AllowEmpty()
                    .DefaultValue(Options.DatabaseName),
                ValueReceiver = (v) => Options.DatabaseName = v,
                SkipChecker = IsAdminTemplate
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>("Enter the admin [green]password[/]:")
                    .AllowEmpty()
                    .DefaultValue(Options.AdminPassword),
                ValueReceiver = (v) => Options.AdminPassword = v,
                SkipChecker = IsAdminTemplate
            }));
        }


        private bool IsAdminTemplate() => Options.Template.Equals(Constants.TEMPLATE_ADMIN, StringComparison.OrdinalIgnoreCase);
    }
}
