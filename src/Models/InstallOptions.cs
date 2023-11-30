namespace Xperience.Xman.Models
{
    public class InstallOptions
    {
        public Version? Version { get; set; }


        public string Template { get; set; } = "kentico-xperience-sample-mvc";


        public string ProjectName { get; set; } = "xbk";


        public bool UseCloud { get; set; } = false;


        public string DatabaseName { get; set; } = "xperience";


        public string? ServerName { get; set; }


        public string AdminPassword { get; set; } = "test";
    }
}