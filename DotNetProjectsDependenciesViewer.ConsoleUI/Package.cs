using System;

namespace DotNetProjectsDependenciesViewer.ConsoleUI
{
    public class Package
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Id
        {
            get
            {
                return Name + " " + Version;
            }
        }

        public string NameWithVersion
        {
            get
            {
                return Name + "-" + Version;
            }
        }

        public Package(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public bool IsEqual(string name, string version)
        {
            return Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && Version.Equals(version, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
