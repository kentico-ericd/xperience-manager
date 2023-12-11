using System.Reflection;

using Newtonsoft.Json;

using Xperience.Xman.Configuration;
using Xperience.Xman.Options;

namespace Xperience.Xman.Services
{
    public class ConfigManager : IConfigManager
    {
        public async Task AddProfile(ToolProfile profile)
        {
            var config = await GetConfig();
            config.Profiles.Add(profile);

            await WriteConfig(config);
        }


        public async Task<bool> TrySetCurrentProfile(string profileName)
        {
            var config = await GetConfig();
            var match = config.Profiles.FirstOrDefault(p => p.ProjectName?.Equals(profileName, StringComparison.OrdinalIgnoreCase) ?? false);
            if (match is null)
            {
                return false;
            }

            config.CurrentProfile = profileName;
            await WriteConfig(config);

            return true;
        }


        public async Task<ToolProfile?> GetCurrentProfile()
        {
            var config = await GetConfig();
            var match = config.Profiles.FirstOrDefault(p => p.ProjectName?.Equals(config.CurrentProfile, StringComparison.OrdinalIgnoreCase) ?? false);
            if (config.Profiles.Count == 1 &&
                (string.IsNullOrEmpty(config.CurrentProfile) || match is null))
            {
                // There's only 1 profile and there was no value stored in config, or non-matching value
                // Select the only profile and save it in the config
                var profile = config.Profiles.FirstOrDefault();
                if (profile is not null)
                {
                    await TrySetCurrentProfile(profile.ProjectName ?? string.Empty);

                    return profile;
                }
            }

            return match;
        }


        public async Task<ToolConfiguration> GetConfig()
        {
            if (!File.Exists(Constants.CONFIG_FILENAME))
            {
                throw new FileNotFoundException($"The configuration file {Constants.CONFIG_FILENAME} was not found.");
            }

            string text = await File.ReadAllTextAsync(Constants.CONFIG_FILENAME);
            var config = JsonConvert.DeserializeObject<ToolConfiguration>(text) ?? throw new JsonReaderException($"The configuration file {Constants.CONFIG_FILENAME} cannot be deserialized.");

            return config;
        }


        public async Task EnsureConfigFile()
        {
            var toolVersion = Assembly.GetExecutingAssembly().GetName().Version ?? throw new InvalidOperationException("The tool version couldn't be retrieved.");
            if (File.Exists(Constants.CONFIG_FILENAME))
            {
                await MigrateConfig(toolVersion);
                return;
            }

            await WriteConfig(new ToolConfiguration
            {
                Version = toolVersion,
                DefaultInstallOptions = new()
            });
        }


        public async Task<InstallOptions> GetDefaultInstallOptions()
        {
            var config = await GetConfig();

            return config.DefaultInstallOptions ?? new();
        }


        public async Task RemoveProfile(string name)
        {
            var config = await GetConfig();

            // For some reason Profiles.Remove() didn't work, make a new list
            var newProfiles = new List<ToolProfile>();
            foreach (var p in config.Profiles)
            {
                if (!p.ProjectName?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    newProfiles.Add(p);
                }
            }

            config.Profiles = newProfiles;

            await WriteConfig(config);
        }


        private async Task MigrateConfig(Version toolVersion)
        {
            var config = await GetConfig();
            if (config.Version?.Equals(toolVersion) ?? false)
            {
                return;
            }

            config.Version = toolVersion;
            // Perform any migrations from old config version to new version here

            await WriteConfig(config);
        }


        private Task WriteConfig(ToolConfiguration config) => File.WriteAllTextAsync(Constants.CONFIG_FILENAME, JsonConvert.SerializeObject(config));
    }
}
