using System.Text;

using Xperience.Xman.Models;
using Xperience.Xman.Steps;

namespace Xperience.Xman.Helpers
{
    /// <summary>
    /// Contains methods for parsing user input into <see cref="InstallOptions"/>.
    /// </summary>
    public class InstallOptionsHelper
    {
        private readonly StepList steps = new();
        private readonly InstallOptions options = new();
        private readonly Dictionary<string, string> TEMPLATES = new() {
            { "Dancing Goat", "kentico-xperience-sample-mvc" },
            { "Boilerplate", "kentico-xperience-mvc" },
            { "Admin customization boilerplate", "kentico-xperience-admin-sample" }
        };
        

        /// <summary>
        /// Initializes a new instance of <see cref="InstallOptionsHelper"/>.
        /// </summary>
        public InstallOptionsHelper()
        {
            var templatePrompt = new StringBuilder($"Which template? Leave empty to use '{options.Template}'\n");
            for (var i = 0; i < TEMPLATES.Count; i++)
            {
                var t = TEMPLATES.ElementAt(i);
                templatePrompt.AppendLine($"[{i}] {t.Key}");
            }

            steps.AddRange(new Step[] {
                new Step($"Which version? Leave empty to use the latest version", SetVersion, "Please enter a valid version, ie '27.0.0'"),
                new Step(templatePrompt.ToString(), SetTemplate, $"\nEnter a number between 0 and {TEMPLATES.Count - 1}", true),
                new Step($"Name your project. Leave empty to use '{options.ProjectName}'", SetProjectName),
                new Step($"Use cloud (Y/N)? Leave empty to use '{options.UseCloud}'", SetCloud, "\nPress Y, N, or Enter", true),
                new Step("Enter the SQL server name", SetServerName, "You must enter a SQL server"),
                new Step($"Enter the database name. Leave empty to use '{options.DatabaseName}'", SetDatabaseName),
                new Step($"Enter the admin password. Leave empty to use '{options.AdminPassword}'", SetPassword)
            });
        }


        /// <summary>
        /// Requests user input to generate an <see cref="InstallOptions"/>.
        /// </summary>
        public InstallOptions GetOptions()
        {
            while (steps.HasNext())
            {
                steps.Current.Execute();
                steps.Next();
            }

            return options;
        }


        private bool SetCloud(string inputKey)
        {
            if (inputKey.Equals(ConsoleKey.Enter.ToString()))
            {
                return true;
            }

            if (!(inputKey.Equals(ConsoleKey.Y.ToString()) || inputKey.Equals(ConsoleKey.N.ToString())))
            {
                return false;
            }

            options.UseCloud = inputKey.Equals(ConsoleKey.Y.ToString());

            return true;
        }


        private bool SetDatabaseName(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                options.DatabaseName = name;
            }

            return true;
        }


        private bool SetPassword(string password)
        {
            if (!String.IsNullOrEmpty(password))
            {
                options.AdminPassword = password;
            }

            return true;
        }


        private bool SetProjectName(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                options.ProjectName = name;
            }

            return true;
        }


        private bool SetVersion(string versionString)
        {
            if (String.IsNullOrEmpty(versionString))
            {
                return true;
            }

            if (Version.TryParse(versionString, out var ver))
            {
                options.Version = ver;
                return true;
            }

            return false;
        }


        private bool SetServerName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }

            options.ServerName = name;

            return true;
        }


        private bool SetTemplate(string inputKey)
        {
            if (inputKey.Equals(ConsoleKey.Enter.ToString()))
            {
                return true;
            }

            var keyNum = inputKey.TrimStart('D');
            if (int.TryParse(keyNum, out var index) && index < TEMPLATES.Count && index >= 0)
            {
                options.Template = TEMPLATES.Values.ElementAt(index);
                return true;
            }

            return false;
        }
    }
}