using Spectre.Console;

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
            Steps.Add(new Step<string>(
                new TextPrompt<string>("Which [green]version[/]? [green](latest)[/]")
                    .AllowEmpty()
                    .ValidationErrorMessage($"[{Constants.ERROR_COLOR}]Please enter a valid version, ie '27.0.0'[/]")
                    .Validate(SetVersion)));
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