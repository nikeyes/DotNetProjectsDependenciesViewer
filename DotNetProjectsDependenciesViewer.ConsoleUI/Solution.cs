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

        public Solution()
        {
            this.Projects = new List<Project>();
        }
    }
}
