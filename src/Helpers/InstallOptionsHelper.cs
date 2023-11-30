using Xperience.Xman.Models;

namespace Xperience.Xman.Helpers
{
    /// <summary>
    /// Contains methods for parsing user input into <see cref="InstallOptions"/>.
    /// </summary>
    public static class InstallOptionsHelper
    {
        private static readonly Dictionary<string, string> TEMPLATES = new() {
            { "Dancing Goat", "kentico-xperience-sample-mvc" },
            { "Boilerplate", "kentico-xperience-mvc" },
            { "Admin customization boilerplate", "kentico-xperience-admin-sample" }
        };


        /// <summary>
        /// Requests user input to generate an <see cref="InstallOptions"/>.
        /// </summary>
        public static InstallOptions GetOptions()
        {
            var options = new InstallOptions();

            if (GetVersion(out var version) && version is not null)
            {
                options.Version = version;
            }

            XConsole.WriteLine("\n");
            if (GetTemplate(out var template, options.Template) && !string.IsNullOrEmpty(template))
            {
                options.Template = template;
            }

            XConsole.WriteLine("\n");
            if (GetProjectName(out var name, options.ProjectName) && !string.IsNullOrEmpty(name))
            {
                options.ProjectName = name;
            }

            XConsole.WriteLine("\n");
            if (GetCloud(out var useCloud, options.UseCloud) && useCloud is not null)
            {
                options.UseCloud = (bool)useCloud;
            }

            XConsole.WriteLine("\n");
            options.ServerName = GetServerName();

            XConsole.WriteLine("\n");
            if (GetDatabaseName(out var dbName, options.DatabaseName) && !string.IsNullOrEmpty(dbName))
            {
                options.DatabaseName = dbName;
            }

            XConsole.WriteLine("\n");
            if (GetPassword(out var password, options.AdminPassword) && !string.IsNullOrEmpty(password))
            {
                options.AdminPassword = password;
            }

            return options;
        }


        private static bool GetPassword(out string? password, string defaultOption)
        {
            XConsole.WriteLine($"Enter the admin password. Leave empty to use '{defaultOption}'");
            var pw = Console.ReadLine();
            if (String.IsNullOrEmpty(pw))
            {
                password = null;
                return false;
            }

            password = pw;
            return true;
        }


        private static bool GetDatabaseName(out string? databaseName, string defaultOption)
        {
            XConsole.WriteLine($"Enter the database name. Leave empty to use '{defaultOption}'");
            var name = Console.ReadLine();
            if (String.IsNullOrEmpty(name))
            {
                databaseName = null;
                return false;
            }

            databaseName = name;
            return true;
        }


        private static string GetServerName()
        {
            XConsole.WriteLine("Enter the SQL server name");
            var name = Console.ReadLine();
            if (String.IsNullOrEmpty(name))
            {
                return GetServerName();
            }

            return name;
        }


        private static bool GetCloud(out bool? useCloud, bool defaultOption)
        {
            XConsole.WriteLine($"Use cloud (Y/N)? Push Enter to use '{defaultOption}'");
            var keyInfo = Console.ReadKey();
            if (keyInfo.Key.Equals(ConsoleKey.Enter))
            {
                useCloud = null;
                return false;
            }

            if (!(keyInfo.Key.Equals(ConsoleKey.Y) || keyInfo.Key.Equals(ConsoleKey.N)))
            {
                XConsole.WriteErrorLine("\nPress Y, N, or Enter");
                return GetCloud(out useCloud, defaultOption);
            }

            useCloud = keyInfo.Key.Equals(ConsoleKey.Y);
            return true;
        }


        private static bool GetProjectName(out string? projectName, string? defaultOption)
        {
            XConsole.WriteLine($"Name your project. Leave empty to use '{defaultOption}'");
            var name = Console.ReadLine();
            if (String.IsNullOrEmpty(name))
            {
                projectName = null;
                return false;
            }

            projectName = name;
            return true;
        }


        private static bool GetVersion(out Version? version)
        {
            XConsole.WriteLine("Which version? Leave empty to use latest version");
            var versionString = Console.ReadLine();
            if (String.IsNullOrEmpty(versionString))
            {
                version = null;
                return false;
            }

            if (Version.TryParse(versionString, out var ver))
            {
                version = ver;
                return true;
            }
            else
            {
                XConsole.WriteErrorLine("Please enter a valid version, ie '27.0.0'");
                return GetVersion(out version);
            }
        }


        private static bool GetTemplate(out string? template, string? defaultOption)
        {
            XConsole.WriteLine($"Which template? Leave empty to use '{defaultOption}'");
            for (var i = 0; i < TEMPLATES.Count; i++)
            {
                var t = TEMPLATES.ElementAt(i);
                XConsole.WriteLine($"[{i}] {t.Key}");
            }

            var templateKey = Console.ReadKey();
            if (templateKey.Key.Equals(ConsoleKey.Enter))
            {
                template = null;
                return false;
            }

            if (int.TryParse(templateKey.KeyChar.ToString(), out var index) && index < TEMPLATES.Count && index >= 0)
            {
                template = TEMPLATES.Values.ElementAt(index);
                return true;
            }
            else
            {
                XConsole.WriteErrorLine($"\nEnter a number between 0 and {TEMPLATES.Count - 1}");
                return GetTemplate(out template, defaultOption);
            }

        }
    }
}