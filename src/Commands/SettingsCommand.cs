using System.Text;
using Spectre.Console;

using Xperience.Xman.Configuration;
using Xperience.Xman.Services;

namespace Xperience.Xman.Commands
{
    /// <summary>
    /// A command which configures the appsettings.json of a project.
    /// </summary>
    public class SettingsCommand : AbstractCommand
    {
        private readonly IAppSettingsManager appSettingsManager;


        public override IEnumerable<string> Keywords => new string[] { "s", "settings" };


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Configures the appsettings.json of a project";


        public override bool RequiresProfile => true;


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal SettingsCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public SettingsCommand(IAppSettingsManager appSettingsManager) => this.appSettingsManager = appSettingsManager;


        public override async Task Execute(ToolProfile? profile, string? action)
        {
            string connStringName = "CMSConnectionString";
            string? connString = await appSettingsManager.GetConnectionString(profile, connStringName);
            if (connString is null)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Unable to load connection string, skipping...[/]");
            }
            else
            {
                AnsiConsole.Write(new Markup($"[{Constants.PROMPT_COLOR} underline]{connStringName}[/]\n{connString}\n\n").Centered());
                bool updateConnection = AnsiConsole.Prompt(new ConfirmationPrompt($"Do you want to change the [{Constants.PROMPT_COLOR}]{connStringName}?[/]")
                {
                    DefaultValue = true
                });
                if (updateConnection)
                {
                    await UpdateConnectionString(profile, connStringName);
                }
            }

            await UpdateSettings(profile);
        }


        public override async Task PostExecute(ToolProfile? profile, string? action)
        {
            AnsiConsole.WriteLine();

            await base.PostExecute(profile, action);
        }


        private bool ConfirmUpdateSettings(string message) => AnsiConsole.Prompt(new ConfirmationPrompt(message)
        {
            DefaultValue = true
        });


        private ConfigurationKey GetNewSettingsKey(IEnumerable<ConfigurationKey> keys)
        {
            var keyToUpdate = AnsiConsole.Prompt(new SelectionPrompt<ConfigurationKey>()
                .Title($"Set which [{Constants.PROMPT_COLOR}]key[/]?")
                .PageSize(10)
                .UseConverter(v => $"{v.KeyName}{(v.ActualValue is not null ? $" ({Truncate(v.ActualValue.ToString(), 12)})" : string.Empty)}")
                .MoreChoicesText("Scroll for more...")
                .AddChoices(keys));

            var header = new StringBuilder($"\n[{Constants.PROMPT_COLOR} underline]{keyToUpdate.KeyName}[/]");
            if (keyToUpdate.ActualValue is not null)
            {
                header.AppendLine($"\nValue: [{Constants.PROMPT_COLOR} underline]{keyToUpdate.ActualValue.ToString().EscapeMarkup()}[/]");
            }

            header.AppendLine($"\n{keyToUpdate.Description}\n");
            AnsiConsole.Write(new Markup(header.ToString()).Centered());

            string newValue = AnsiConsole.Prompt(new TextPrompt<string>($"Enter the new value for [{Constants.PROMPT_COLOR}]{keyToUpdate.KeyName}[/]:"));
            object converted = Convert.ChangeType(newValue, keyToUpdate.ValueType) ?? throw new InvalidCastException($"The key value cannot be cast into type {keyToUpdate.ValueType.Name}");
            keyToUpdate.ActualValue = converted;

            return keyToUpdate;
        }


        private string? Truncate(string? value, int maxLength, string truncationSuffix = "...") => value?.Length > maxLength
                ? value[..maxLength] + truncationSuffix
                : value;


        private async Task UpdateConnectionString(ToolProfile? profile, string name)
        {
            string newConnString = AnsiConsole.Prompt(new TextPrompt<string>($"Enter new [{Constants.PROMPT_COLOR}]connection string[/]:"));
            await appSettingsManager.SetConnectionString(profile, name, newConnString);

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Connection string updated![/]\n");
        }


        private async Task UpdateSettings(ToolProfile? profile)
        {
            bool updateSettings = ConfirmUpdateSettings($"Do you want to update your [{Constants.PROMPT_COLOR}]configuration keys[/]?");
            if (!updateSettings)
            {
                return;
            }

            var keys = await appSettingsManager.GetConfigurationKeys(profile);
            while (updateSettings)
            {
                var updatedKey = GetNewSettingsKey(keys);
                if (updatedKey.ActualValue is null)
                {
                    throw new InvalidOperationException($"Failed to set new value for key {updatedKey.KeyName}");
                }

                await appSettingsManager.SetKeyValue(profile, updatedKey.KeyName, updatedKey.ActualValue);
                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updated the {updatedKey.KeyName} key![/]\n");

                updateSettings = ConfirmUpdateSettings($"Update another [{Constants.PROMPT_COLOR}]configuration key[/]?");
            }
        }
    }
}
