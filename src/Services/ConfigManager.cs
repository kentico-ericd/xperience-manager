using Newtonsoft.Json;

using Xperience.Xman.Configuration;
using Xperience.Xman.Options;

namespace Xperience.Xman.Services
{
    public class ConfigManager : IConfigManager
    {
        public async Task AddProfile(Profile profile)
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


        public async Task<Profile?> GetCurrentProfile()
        {
            var config = await GetConfig();
            if (string.IsNullOrEmpty(config.CurrentProfile) && config.Profiles.Count == 1)
            {
                return config.Profiles.First();
            }

            return config.Profiles.FirstOrDefault(p => p.ProjectName?.Equals(config.CurrentProfile, StringComparison.OrdinalIgnoreCase) ?? false);
        }


        public async Task<ToolConfiguration> GetConfig()
        {
            if (!File.Exists(Constants.CONFIG_FILENAME))
            {
                throw new FileNotFoundException($"The configuration file {Constants.CONFIG_FILENAME} was not found.");
            }

            string text = await File.ReadAllTextAsync(Constants.CONFIG_FILENAME);
            var config = JsonConvert.DeserializeObject<ToolConfiguration>(text) ?? throw new InvalidOperationException($"The configuration file {Constants.CONFIG_FILENAME} cannot be deserialized.");

            return config;
        }


        public Task EnsureConfigFile()
        {
            if (File.Exists(Constants.CONFIG_FILENAME))
            {
                return Task.CompletedTask;
            }

            return WriteConfig(new ToolConfiguration
            {
                DefaultInstallOptions = new()
            });
        }


        public async Task<InstallOptions> GetDefaultInstallOptions()
        {
            var config = await GetConfig();

            return config.DefaultInstallOptions ?? new();
        }


        private Task WriteConfig(ToolConfiguration config) => File.WriteAllTextAsync(Constants.CONFIG_FILENAME, JsonConvert.SerializeObject(config));
    }
}
