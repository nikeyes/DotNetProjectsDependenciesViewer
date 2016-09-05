using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProjectsDependenciesViewer.ConsoleUI
{
    public class Project
    {
        public string PathAndName { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public List<Project> Projects { get; private set; }
        public List<Library> Libraries { get; private set; }
        public List<Package> Packages { get; private set; }

        public Project(String filePathAndName)
        {
            this.Projects = new List<Project>();
            this.Libraries = new List<Library>();
            this.Packages = new List<Package>();

            PathAndName = filePathAndName;
            Path = System.IO.Path.GetDirectoryName(filePathAndName);
            Name = System.IO.Path.GetFileNameWithoutExtension(filePathAndName);
        }
    }

}
