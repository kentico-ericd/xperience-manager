﻿using System.Reflection;

using Newtonsoft.Json;

using Xperience.Xman.Configuration;
using Xperience.Xman.Options;

namespace Xperience.Xman.Services
{
    public class ConfigManager : IConfigManager
    {
        private ToolConfiguration? configuration;


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
            if (configuration is not null)
            {
                return configuration;
            }

            if (!File.Exists(Constants.CONFIG_FILENAME))
            {
                throw new FileNotFoundException($"The configuration file {Constants.CONFIG_FILENAME} was not found.");
            }

            string text = await File.ReadAllTextAsync(Constants.CONFIG_FILENAME);
            var config = JsonConvert.DeserializeObject<ToolConfiguration>(text) ?? throw new InvalidOperationException($"The configuration file {Constants.CONFIG_FILENAME} cannot be deserialized.");

            configuration = config;

            return config;
        }


        public Task EnsureConfigFile()
        {
            if (File.Exists(Constants.CONFIG_FILENAME))
            {
                return Task.CompletedTask;
            }

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? throw new InvalidOperationException("The tool version couldn't be retrieved.");

            return WriteConfig(new ToolConfiguration
            {
                Version = currentVersion,
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
            var newProfiles = new List<Profile>();
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


        private Task WriteConfig(ToolConfiguration config) => File.WriteAllTextAsync(Constants.CONFIG_FILENAME, JsonConvert.SerializeObject(config));
    }
}
