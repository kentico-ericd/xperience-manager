using Xperience.Xman.Configuration;

namespace Xperience.Xman.Services
{
    public interface IAppSettingsManager : IService
    {
        public Task<string?> GetConnectionString(ToolProfile profile, string name);


        public Task<IEnumerable<ConfigurationKey>> GetConfigurationKeys(ToolProfile profile);


        public Task SetConnectionString(ToolProfile profile, string name, string connectionString);


        public Task SetKeyValue(ToolProfile profile, string keyName, object value);
    }
}
