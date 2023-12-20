using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private ToolProfile? profile;
        private readonly IConfigManager configManager;
        private readonly IAppSettingsManager appSettingsManager;


        public override IEnumerable<string> Keywords => new string[] { "s", "settings" };


        public override IEnumerable<string> Parameters => Enumerable.Empty<string>();


        public override string Description => "Configures the appsettings.json of a project";


        /// <summary>
        /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected
        /// into the constuctor.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal SettingsCommand()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }


        public SettingsCommand(IConfigManager configManager, IAppSettingsManager appSettingsManager)
        {
            this.configManager = configManager;
            this.appSettingsManager = appSettingsManager;
        }


        public override async Task PreExecute(string[] args)
        {
            profile = await configManager.GetCurrentProfile() ?? throw new InvalidOperationException("There is no active profile.");
            PrintCurrentProfile(profile);

            AnsiConsole.WriteLine();
        }


        public override async Task Execute(string[] args)
        {
            if (profile is null)
            {
                throw new InvalidOperationException("There is no active profile.");
            }

            string connStringName = "CMSConnectionString";
            string? connString = await appSettingsManager.GetConnectionString(profile, connStringName);
            if (connString is null)
            {
                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Unable to load connection string, skipping...[/]");
            }
            else
            {
                AnsiConsole.MarkupLineInterpolated($"Current {connStringName}: {connString}");
                bool updateConnection = AnsiConsole.Prompt(new ConfirmationPrompt("Do you want to change your [green]connection string?[/]")
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


        public override async Task PostExecute(string[] args)
        {
            AnsiConsole.WriteLine();

            await base.PostExecute(args);
        }


        private bool ConfirmUpdateSettings(string message) => AnsiConsole.Prompt(new ConfirmationPrompt(message)
        {
            DefaultValue = true
        });


        private ConfigurationKey GetNewSettingsKey(IEnumerable<ConfigurationKey> keys)
        {
            // TODO: The formatting here kinda sucks
            var keyToUpdate = AnsiConsole.Prompt(new SelectionPrompt<ConfigurationKey>()
                    .Title($"Set which [{Constants.SUCCESS_COLOR}]key[/]?")
                    .PageSize(10)
                    .UseConverter(v =>
                    {
                        string line = $"[{Constants.SUCCESS_COLOR}][[{v.KeyName}]][/] {v.Description}";
                        if (v.ActualValue is not null)
                        {
                            line += $"\n     - Existing value: [{Constants.EMPHASIS_COLOR}]{v.ActualValue}[/]";
                        }

                        return line;
                    })
                    .MoreChoicesText("Scroll for more...")
                    .AddChoices(keys));
            string newValue = AnsiConsole.Prompt(new TextPrompt<string>($"Enter the new value for [{Constants.SUCCESS_COLOR}]{keyToUpdate.KeyName}[/]:"));
            object converted = Convert.ChangeType(newValue, keyToUpdate.ValueType) ?? throw new InvalidCastException($"The key value cannot be cast into type {keyToUpdate.ValueType.Name}");
            keyToUpdate.ActualValue = converted;

            return keyToUpdate;
        }


        private async Task UpdateConnectionString(ToolProfile profile, string name)
        {
            string newConnString = AnsiConsole.Prompt(new TextPrompt<string>($"Enter new [{Constants.SUCCESS_COLOR}]connection string[/]:"));
            await appSettingsManager.SetConnectionString(profile, name, newConnString);

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Connection string updated![/]");
        }


        private async Task UpdateSettings(ToolProfile profile)
        {
            bool updateSettings = ConfirmUpdateSettings($"Do you want to update your [{Constants.SUCCESS_COLOR}]configuration keys[/]?");
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
                AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updated the {updatedKey.KeyName} key![/]");

                updateSettings = ConfirmUpdateSettings($"Update another [{Constants.SUCCESS_COLOR}]configuration keys[/]?");
            }
        }
    }
}
