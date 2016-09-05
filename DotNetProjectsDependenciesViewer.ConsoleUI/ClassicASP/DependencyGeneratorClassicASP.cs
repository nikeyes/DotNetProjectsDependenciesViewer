using DotNetProjectsDependenciesViewer.ConsoleUI.ClassicASP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DotNetProjectsDependenciesViewer.ConsoleUI.ClassicASP
{
    public class DependencyGeneratorClassicASP
    {
        private List<ClassicASPFile> _allClassicASPFiles = new List<ClassicASPFile>();
        
        public DependencyGeneratorClassicASP(string rootFolder)
        {
            LoadAllClassicASPFiles(rootFolder);

            string path = Path.Combine(rootFolder, "Dependencies.dgml");
            DGMLPackageDependenciesClassicASP packageDependencies = new DGMLPackageDependenciesClassicASP();
           
            packageDependencies.generateDGML(path, _allClassicASPFiles);
            
        }

        private void LoadAllClassicASPFiles(string rootFolder)
        {
            ArrayList classicASPFiles = new ArrayList();

            classicASPFiles.AddRange(Directory.GetFiles(rootFolder, "*.*asp", SearchOption.AllDirectories));
            classicASPFiles.AddRange(Directory.GetFiles(rootFolder, "*.*inc", SearchOption.AllDirectories));

            foreach (String file in classicASPFiles)
            {
                ClassicASPFile classicASPFile = new ClassicASPFile(file);

                _allClassicASPFiles.Add(classicASPFile);
            }
        }
    }
}
