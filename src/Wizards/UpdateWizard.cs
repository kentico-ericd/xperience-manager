using Spectre.Console;

using Xperience.Xman.Helpers;
using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="UpdateOptions"/> for updating Xperience by Kentico.
    /// </summary>
    public class UpdateWizard : AbstractWizard<UpdateOptions>
    {
        public override async Task InitSteps()
        {
            var versions = await NuGetVersionHelper.GetPackageVersions("kentico.xperience.templates");
            var filtered = versions.Where(v => !v.IsPrerelease && !v.IsLegacyVersion && v.Major >= 25)
                .Select(v => v.Version)
                .OrderByDescending(v => v);

            Steps.Add(new Step<Version>(
                new SelectionPrompt<Version>()
                    .Title("Which [green]version[/]?")
                    .PageSize(10)
                    .UseConverter(v => $"{v.Major}.{v.Minor}.{v.Build}")
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(filtered),
                (v) => Options.Version = v));
        }
    }
}
