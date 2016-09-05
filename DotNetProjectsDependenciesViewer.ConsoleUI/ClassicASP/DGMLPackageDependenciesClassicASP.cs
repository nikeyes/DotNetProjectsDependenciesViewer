using DotNetProjectsDependenciesViewer.ConsoleUI.ClassicASP;
using DotNetProjectsDependenciesViewer.ConsoleUI.DGML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProjectsDependenciesViewer.ConsoleUI.ClassicASP
{
    public class DGMLPackageDependenciesClassicASP
    {
        public void generateDGML(string filename, List<ClassicASPFile> classicASPFiles)
        {
            DGMLWriter dgml = new DGMLWriter();
            AddClassicASPFilesNodes(dgml, classicASPFiles);
            AddStyleToGraph(dgml);

            dgml.Serialize(filename);
        }

        private void AddClassicASPFilesNodes(DGMLWriter dgml, List<ClassicASPFile> classicASPFiles)
        {
            foreach (ClassicASPFile classicASPFile in classicASPFiles)
            {
                dgml.AddNode(classicASPFile.PathAndName, null, null, "ClassicASPFile");
            }

            AddLinksBetweenNodes(dgml, classicASPFiles);
        }

        private void AddLinksBetweenNodes(DGMLWriter dgml, List<ClassicASPFile> classicASPFiles)
        {
            foreach (ClassicASPFile classicASPFile in classicASPFiles)
            {
                foreach (String include in classicASPFile.Includes)
                {
                   dgml.AddLink(classicASPFile.PathAndName, include, null, "File Reference");
                }
            }
        }

        private static void AddStyleToGraph(DGMLWriter dgml)
        {
            dgml.AddStyle("ClassicASPFile", "Blue");
            dgml.AddStyle("Package", "Purple");
            dgml.AddStyle("PackageGroup", "Plum");
            dgml.AddStyle("Library", "Green");
            dgml.AddStyle("GAC Library", "LightGreen");
        }
    }
}
