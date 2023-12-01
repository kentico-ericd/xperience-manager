using Newtonsoft.Json;

using Xperience.Xman.Commands;
using Xperience.Xman.Options;

namespace Xperience.Xman.Helpers
{
    /// <summary>
    /// Contains methods for managing the configuration file used in <see cref="InstallCommand"/>.
    /// </summary>
    public static class ConfigFileHelper
    {
        /// <summary>
        /// Stores the <see cref="InstallOptions"/> in the configuration file.
        /// </summary>
        public static void CreateConfigFile(InstallOptions options)
        {
            File.WriteAllText(Constants.CONFIG_FILENAME, JsonConvert.SerializeObject(options));
        }


        /// <summary>
        /// Loads <see cref="InstallOptions"/> from the configuration file.
        /// </summary>
        /// <returns></returns>
        public static InstallOptions? GetOptionsFromConfig()
        {
            if (!File.Exists(Constants.CONFIG_FILENAME))
            {
                return null;
            }

            try
            {
                var text = File.ReadAllText(Constants.CONFIG_FILENAME);

                return JsonConvert.DeserializeObject<InstallOptions>(text);
            }
            catch
            {
                return null;
            }
        }
    }
}
