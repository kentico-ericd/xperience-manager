using Spectre.Console;

using Xperience.Xman.Options;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Wizards
{
    public class RepositoryConfigurationWizard : AbstractWizard<RepositoryConfiguration>
    {
        private bool changeIncluded;
        private bool changeExcluded;
        private readonly IEnumerable<string> restoreModes = new string[]
        {
            "Create",
            "CreateUpdate",
            "Full"
        };


        public override Task InitSteps()
        {
            Steps.Add(new Step<string>(new()
            {
                Prompt = new SelectionPrompt<string>()
                    .Title($"Which [green]RestoreMode[/]? [green]({Options.RestoreMode})[/]")
                    .PageSize(10)
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(restoreModes),
                ValueReceiver = (v) => Options.RestoreMode = v
            }));

            Steps.Add(new Step<bool>(new()
            {
                Prompt = new ConfirmationPrompt($"[green]Included[/] object types: {string.Join(";", Options.IncludedObjectTypes ?? Enumerable.Empty<string>())}\nWould you like to change them?")
                {
                    DefaultValue = false
                },
                ValueReceiver = (v) => changeIncluded = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>("Enter new [green]included[/] object types:")
                    .AllowEmpty(),
                ValueReceiver = (v) => Options.IncludedObjectTypes = new List<string>(v.Split(';')),
                SkipChecker = () => !changeIncluded
            }));

            Steps.Add(new Step<bool>(new()
            {
                Prompt = new ConfirmationPrompt($"\n[green]Excluded[/] object types: {string.Join(";", Options.ExcludedObjectTypes ?? Enumerable.Empty<string>())}\nWould you like to change them?")
                {
                    DefaultValue = false
                },
                ValueReceiver = (v) => changeExcluded = v
            }));

            Steps.Add(new Step<string>(new()
            {
                Prompt = new TextPrompt<string>("Enter new [green]excluded[/] object types:")
                    .AllowEmpty(),
                ValueReceiver = (v) => Options.ExcludedObjectTypes = new List<string>(v.Split(';')),
                SkipChecker = () => !changeExcluded
            }));

            return Task.CompletedTask;
        }
    }
}
