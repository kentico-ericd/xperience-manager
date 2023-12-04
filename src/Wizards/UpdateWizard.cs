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
        /// <summary>
        /// Initializes a new instance of <see cref="UpdateWizard"/>.
        /// </summary>
        public UpdateWizard()
        {
            InitSteps(); 
        }


        protected override void InitSteps()
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
        }
    }
}