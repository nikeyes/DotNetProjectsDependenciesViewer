using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DotNetProjectsDependenciesViewer.ConsoleUI.ClassicASP
{
    public class ClassicASPFile
    {
        public string PathAndName { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string AllText { get; set; }
        public List<string> Includes { get; set; }

        public ClassicASPFile(string filePathAndName)
        {
            Includes = new List<string>();

            PathAndName = filePathAndName;
            Name = System.IO.Path.GetFileName(filePathAndName);
            Path = System.IO.Path.GetDirectoryName(filePathAndName);
            AllText = File.ReadAllText(filePathAndName);

            SearchIncludes(Path);
        }

        private void SearchIncludes(string basePath)
        {
            var regex = new Regex("#include\\W+(file|virtual)=\"([^\"]+)\"", RegexOptions.IgnoreCase);
            var matchResult = regex.Match(AllText);
            while (matchResult.Success)
            {
                string pathInclude = System.IO.Path.Combine(basePath, matchResult.Groups[1].Value);
                pathInclude = pathInclude.Replace("/", @"\");
                pathInclude = System.IO.Path.GetFullPath(pathInclude);

                Includes.Add(pathInclude);
                matchResult = matchResult.NextMatch();
            }
        }
    }
}
