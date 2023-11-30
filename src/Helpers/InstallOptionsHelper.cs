using Xperience.Xman.Models;

namespace Xperience.Xman.Helpers
{
    public static class InstallOptionsHelper
    {
        private static readonly Dictionary<string, string> TEMPLATES = new Dictionary<string, string> {
            { "Dancing Goat", "kentico-xperience-sample-mvc" },
            { "Boilerplate", "kentico-xperience-mvc" },
            { "Admin customization boilerplate", "kentico-xperience-admin-sample" }
        };


        public static InstallOptions GetOptions()
        {
            var options = new InstallOptions();

            if (GetVersion(out var version) && version is not null)
            {
                options.Version = version;
            }

            if (GetTemplate(out var template, options.Template) && !string.IsNullOrEmpty(template))
            {
                options.Template = template;
            }

            if (GetProjectName(out var name, options.ProjectName) && !string.IsNullOrEmpty(name))
            {
                options.ProjectName = name;
            }

            if (GetCloud(out var useCloud, options.UseCloud) && useCloud is not null)
            {
                options.UseCloud = (bool)useCloud;
            }

            options.ServerName = GetServerName();
            options.DatabaseName = GetDatabaseName();

            if (GetPassword(out var password, options.AdminPassword) && !string.IsNullOrEmpty(password))
            {
                options.AdminPassword = password;
            }

            return options;
        }


        private static bool GetPassword(out string? password, string defaultOption)
        {
            Console.WriteLine($"Enter the admin password. Leave empty to use '{defaultOption}'");
            var pw = Console.ReadLine();
            if (string.IsNullOrEmpty(pw))
            {
                password = null;
                return false;
            }

            password = pw;
            return true;
        }


        private static string? GetDatabaseName()
        {
            Console.WriteLine("Enter the database name. Leave empty to use the deafult name");
            var name = Console.ReadLine();

            return string.IsNullOrEmpty(name) ? null : name;
        }


        private static string GetServerName()
        {
            Console.WriteLine("Enter the SQL server name");
            var name = Console.ReadLine();
            if (string.IsNullOrEmpty(name))
            {
                return GetServerName();
            }

            return name;
        }


        private static bool GetCloud(out bool? useCloud, bool defaultOption)
        {
            Console.WriteLine($"Use cloud (Y/N)? Push Enter to use '{defaultOption}'");
            var keyInfo = Console.ReadKey();
            if (keyInfo.Key.Equals(ConsoleKey.Enter))
            {
                useCloud = null;
                return false;
            }

            useCloud = keyInfo.Key.Equals(ConsoleKey.Y);
            return true;
        }


        private static bool GetProjectName(out string? projectName, string defaultOption)
        {
            Console.WriteLine($"Name your project. Leave empty to use '{defaultOption}'");
            var name = Console.ReadLine();
            if (string.IsNullOrEmpty(name))
            {
                projectName = null;
                return false;
            }

            projectName = name;
            return true;
        }


        private static bool GetVersion(out Version? version)
        {
            Console.WriteLine("Which version? Leave empty to use latest version");
            var versionString = Console.ReadLine();
            if (string.IsNullOrEmpty(versionString))
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
                Console.WriteLine("Please enter a valid version, ie '27.0.0'");
                return GetVersion(out version);
            }
        }


        private static bool GetTemplate(out string? template, string defaultOption)
        {
            Console.WriteLine($"Which template? Leave empty to use '{defaultOption}'");
            for (var i = 0; i < TEMPLATES.Count; i++)
            {
                var t = TEMPLATES.ElementAt(i);
                Console.WriteLine($"[{i}] {t.Key}");
            }

            var templateString = Console.ReadLine();
            if (string.IsNullOrEmpty(templateString))
            {
                template = null;
                return false;
            }

            if (int.TryParse(templateString, out var index) && index < TEMPLATES.Count && index >= 0)
            {
                template = TEMPLATES.Values.ElementAt(index);
                return true;
            }
            else
            {
                Console.WriteLine($"Enter a number between 0 and {TEMPLATES.Count - 1}");
                return GetTemplate(out template, defaultOption);
            }

        }
    }
}