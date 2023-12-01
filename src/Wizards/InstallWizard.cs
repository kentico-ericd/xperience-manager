using Spectre.Console;

using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="InstallOptions"/> for installing Xperience by Kentico.
    /// </summary>
    public class InstallWizard : AbstractWizard<InstallOptions>
    {
        private readonly IEnumerable<string> TEMPLATES = new string[] {
            "kentico-xperience-sample-mvc",
            "kentico-xperience-mvc",
            "kentico-xperience-admin-sample"
        };


        /// <summary>
        /// Initializes a new instance of <see cref="InstallWizard"/>.
        /// </summary>
        public InstallWizard()
        {
            InitSteps(); 
        }


        protected override void InitSteps()
        {
            Steps.Add(new Step<string>(
                new TextPrompt<string>("Which [green]version[/]? [green](latest)[/]")
                    .AllowEmpty()
                    .ValidationErrorMessage($"[{Constants.ERROR_COLOR}]Please enter a valid version, ie '27.0.0'[/]")
                    .Validate(SetVersion)));

            Steps.Add(new Step<string>(
                new SelectionPrompt<string>()
                    .Title("Which [green]template[/]?")
                    .AddChoices(TEMPLATES),
                (v) => Options.Template = v.ToString()));

            Steps.Add(new Step<string>(
                new TextPrompt<string>("Give your project a [green]name[/]?")
                    .DefaultValue(Options.ProjectName),
                (v) => Options.ProjectName = v));

            var cloudPrompt = new ConfirmationPrompt("Prepare for [green]cloud[/] deployment?");
            cloudPrompt.DefaultValue = Options.UseCloud;
            Steps.Add(new Step<bool>(cloudPrompt, (v) => Options.UseCloud = v));

            Steps.Add(new Step<string>(
                new TextPrompt<string>("Enter the [green]SQL server[/] name"),
                (v) => Options.ServerName = v));

            Steps.Add(new Step<string>(
                new TextPrompt<string>("Enter the [green]database[/] name")
                    .AllowEmpty()
                    .DefaultValue(Options.DatabaseName),
                (v) => Options.DatabaseName = v));

            Steps.Add(new Step<string>(
                new TextPrompt<string>("Enter the admin [green]password[/]")
                    .AllowEmpty()
                    .DefaultValue(Options.AdminPassword),
                (v) => Options.AdminPassword = v));
        }


        private bool SetVersion(string versionString)
        {
            if (string.IsNullOrEmpty(versionString))
            {
                return true;
            }

            if (Version.TryParse(versionString, out var ver))
            {
                Options.Version = ver;
                return true;
            }

            return false;
        }
    }
}