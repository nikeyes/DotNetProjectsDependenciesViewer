using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProjectsDependenciesViewer.ConsoleUI
{
    public class Solution
    {
        public string PathAndName { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public List<Project> Projects { get; private set; }

        public Solution(String filePathAndName)
        {
            this.Projects = new List<Project>();
            PathAndName = filePathAndName;
            Name = System.IO.Path.GetFileNameWithoutExtension(filePathAndName);
            Path = System.IO.Path.GetDirectoryName(filePathAndName);
        }
    }
}
