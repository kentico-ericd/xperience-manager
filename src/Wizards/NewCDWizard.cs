using Spectre.Console;

using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    /// <summary>
    /// A wizard which generates an <see cref="NewCDOptions"/> for creating new Continuous Development profiles.
    /// </summary>
    public class NewCDWizard : AbstractWizard<NewCDOptions>
    {
        public override Task InitSteps()
        {
            string defaultRootPath = Path.Combine(Environment.CurrentDirectory, Constants.CD_CONFIG_DIR);
            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>("Enter the [green]full path[/] to the CD configuration folder:")
                    .DefaultValue(defaultRootPath),
                ValueReceiver = (v) => Options.RootConfigPath = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>("Enter the [green]environment[/] name:"),
                ValueReceiver = (v) => Options.EnvironmentName = v
            }));

            return Task.CompletedTask;
        }
    }
}
