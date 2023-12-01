using Spectre.Console;

using Xperience.Xman.Models;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Helpers
{
    /// <summary>
    /// Contains methods for parsing user input into <see cref="InstallOptions"/>.
    /// </summary>
    public class InstallOptionsHelper
    {
        private readonly StepList steps = new();
        private readonly InstallOptions options = new();
        private readonly IEnumerable<string> TEMPLATES = new string[] {
            "kentico-xperience-sample-mvc",
            "kentico-xperience-mvc",
            "kentico-xperience-admin-sample"
        };
        

        /// <summary>
        /// Initializes a new instance of <see cref="InstallOptionsHelper"/>.
        /// </summary>
        public InstallOptionsHelper()
        {
            steps.Add(new Step<string>(
                new TextPrompt<string>("Which [green]version[/]? [green](latest)[/]")
                    .AllowEmpty()
                    .ValidationErrorMessage($"[{Constants.ERROR_COLOR}]Please enter a valid version, ie '27.0.0'[/]")
                    .Validate(SetVersion)));

            steps.Add(new Step<string>(
                new SelectionPrompt<string>()
                    .Title("Which [green]template[/]?")
                    .AddChoices(TEMPLATES),
                (v) => options.Template = v.ToString()));

            steps.Add(new Step<string>(
                new TextPrompt<string>("Give your project a [green]name[/]?")
                    .DefaultValue(options.ProjectName),
                (v) => options.ProjectName = v));

            var cloudPrompt = new ConfirmationPrompt("Prepare for [green]cloud[/] deployment?");
            cloudPrompt.DefaultValue = options.UseCloud;
            steps.Add(new Step<bool>(cloudPrompt, (v) => options.UseCloud = v));

            steps.Add(new Step<string>(
                new TextPrompt<string>("Enter the [green]SQL server[/] name"),
                (v) => options.ServerName = v));

            steps.Add(new Step<string>(
                new TextPrompt<string>("Enter the [green]database[/] name")
                    .AllowEmpty()
                    .DefaultValue(options.DatabaseName),
                (v) => options.ServerName = v));

            steps.Add(new Step<string>(
                new TextPrompt<string>("Enter the admin [green]password[/]")
                    .AllowEmpty()
                    .DefaultValue(options.AdminPassword),
                (v) => options.AdminPassword = v));
        }


        /// <summary>
        /// Requests user input to generate an <see cref="InstallOptions"/>.
        /// </summary>
        public InstallOptions GetOptions()
        {
            do
            {
                steps.Current.Execute();
            } while (steps.Next());

            return options;
        }


        private bool SetVersion(string versionString)
        {
            if (String.IsNullOrEmpty(versionString))
            {
                return true;
            }

            if (Version.TryParse(versionString, out var ver))
            {
                options.Version = ver;
                return true;
            }

            return false;
        }
    }
}