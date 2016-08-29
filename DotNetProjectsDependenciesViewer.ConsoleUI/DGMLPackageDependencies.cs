using DotNetProjectsDependenciesViewer.ConsoleUI.DGML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetProjectsDependenciesViewer.ConsoleUI
{
    public class DGMLPackageDependencies
    {
        public void generateDGML(string filename, List<Solution> solutions, List<Project> projects, List<Package> packages, List<Library> libraries)
        {
            DGMLWriter dgml = new DGMLWriter();
            AddProjectsNodes(dgml, projects);
            AddPackagesNodes(dgml, packages);
            AddLibrariesNodes(dgml, libraries);
            AddLinksBetweenNodes(dgml, solutions);
            AddStyleToGraph(dgml);

            dgml.Serialize(filename);
        }

        private void AddPackagesNodes(DGMLWriter dgml, List<Package> packages)
        {
            foreach (Package package in packages)
            {
                dgml.AddNode(package.Id, null, null, "Package");
            }

            AddGroupsForDiferentsVersionOfPackages(dgml, packages);
        }

        private void AddProjectsNodes(DGMLWriter dgml, List<Project> projects)
        {
            foreach (Project project in projects)
            {
                dgml.AddNode(project.Name, null, null, "Project");
            }
        }

        private void AddLibrariesNodes(DGMLWriter dgml, List<Library> libraries)
        {
            dgml.AddNode("GlobalAssemblyCache", "Global Assembly Cache", "Collapsed", "GAC Library");
            dgml.AddNode("LocalLibraries", "Local Libraries", "Expanded", "Libraries");

            foreach (Library library in libraries)
            {
                if (library.IsGAC)
                {
                    dgml.AddNode(library.Name, null, null, "GAC Library");
                    dgml.AddLink("GlobalAssemblyCache", library.Name, null, "Contains");
                }
                else
                {
                    dgml.AddNode(library.Name, null, null, "Library");
                    dgml.AddLink("LocalLibraries", library.Name, null, "Contains");
                }
            }
        }

        private void AddGroupsForDiferentsVersionOfPackages(DGMLWriter dgml, List<Package> packages)
        {
            var groupedPackages = packages
                    .GroupBy(u => u.Name)
                    .Select(grp => grp.ToList())
                    .ToList();

            foreach (List<Package> packageGrouped in groupedPackages)
            {
                dgml.AddNode(packageGrouped[0].Name, packageGrouped[0].Name, "Expanded", "PackageGroup");

                foreach (Package package in packageGrouped)
                {
                    dgml.AddLink(package.Name, package.Id, null, "Contains");
                }
                //dgml.AddLink(new DGMLWriter.Link(project.Name, package.Name + " " + package.Version, null, "Installed Package"));
            }
        }

        private void AddLinksBetweenNodes(DGMLWriter dgml, List<Solution> solutions)
        {
            foreach (Solution solution in solutions)
            {
                dgml.AddNode(solution.Name, solution.Name, "Expanded", "Solution");

                foreach (Project project in solution.Projects)
                {
                    dgml.AddLink(solution.Name, project.Name, null, "Contains");

                    foreach (Project projectChild in project.Projects)
                    {
                        dgml.AddLink(project.Name, projectChild.Name, null, "Project Reference");
                    }

                    foreach (Package package in project.Packages)
                    {
                        dgml.AddLink(project.Name, package.Id, null, "Installed Package");
                    }

                    foreach (Library library in project.Libraries)
                    {
                        dgml.AddLink(project.Name, library.Name, null, "Library Reference");
                    }
                }
            }
        }

        private static void AddStyleToGraph(DGMLWriter dgml)
        {
            dgml.AddStyle("Project", "Blue");
            dgml.AddStyle("Package", "Purple");
            dgml.AddStyle("PackageGroup", "Plum");
            dgml.AddStyle("Library", "Green");
            dgml.AddStyle("GAC Library", "LightGreen");
        }
    }
}
