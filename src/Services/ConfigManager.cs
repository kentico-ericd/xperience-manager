﻿using Newtonsoft.Json;

using Xperience.Xman.Configuration;
using Xperience.Xman.Options;

namespace Xperience.Xman.Services
{
    public class ConfigManager : IConfigManager
    {
        public async Task AddProfile(InstallOptions options)
        {
            var config = await ReadConfig();
            config.Profiles.Add(new()
            {
                WorkingDirectory = options.ProjectName
            });

            await WriteConfig(config);
        }


        public Task EnsureConfigFile()
        {
            if (File.Exists(Constants.CONFIG_FILENAME))
            {
                return Task.CompletedTask;
            }

            return WriteConfig(new ToolConfiguration());
        }


        private async Task<ToolConfiguration> ReadConfig()
        {
            if (File.Exists(Constants.CONFIG_FILENAME))
            {
                throw new FileNotFoundException($"The configuration file {Constants.CONFIG_FILENAME} was not found.");
            }

            string text = await File.ReadAllTextAsync(Constants.CONFIG_FILENAME);
            var config = JsonConvert.DeserializeObject<ToolConfiguration>(text) ?? throw new InvalidOperationException($"The configuration file {Constants.CONFIG_FILENAME} cannot be deserialized.");

            return config;
        }


        private Task WriteConfig(ToolConfiguration config) => File.WriteAllTextAsync(Constants.CONFIG_FILENAME, JsonConvert.SerializeObject(config));
    }
}
