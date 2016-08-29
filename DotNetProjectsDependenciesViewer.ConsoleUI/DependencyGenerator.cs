using DotNetProjectsDependenciesViewer.ConsoleUI.DGML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DotNetProjectsDependenciesViewer.ConsoleUI
{
    public class DependencyGenerator
    {
        private List<Solution> _allSolutions = new List<Solution>();
        private List<Project> _allProjects = new List<Project>();
        private List<Package> _allPackages = new List<Package>();
        private List<Library> _allLibraries = new List<Library>();

        public DependencyGenerator(string rootFolder)
        {
            string[] projectExtensionExclusions = new[] { ".vdproj", ".ndproj", ".wdproj", ".shfbproj", ".sqlproj" };
            //string rootFolder = @"D:\desarrollo\git\repos";

            _allSolutions = new List<Solution>();
            _allProjects = new List<Project>();
            _allPackages = new List<Package>();

            LoadAllSolutions(rootFolder, projectExtensionExclusions);

            LoadAllLibrariesForSolutions();

            GenerateDGML(Path.Combine(rootFolder, "Dependencies.dgml"));
        }

        private void LoadAllSolutions(string rootFolder, string[] projectExtensionExclusions)
        {
            String[] solitionsFile = Directory.GetFiles(rootFolder, "*.*sln", SearchOption.AllDirectories);

            foreach (String sln in solitionsFile)
            {
                Solution solution = new Solution
                {
                    PathAndName = sln,
                    Name = Path.GetFileNameWithoutExtension(sln),
                    Path = Path.GetDirectoryName(sln)
                };

                LoadProjectsForSolution(solution, projectExtensionExclusions);

                _allSolutions.Add(solution);
            }
        }

        private void LoadProjectsForSolution(Solution solution, String[] projectExtensionExclusions)
        {
            IEnumerable<String> projectFiles = Directory.GetFiles(solution.Path, "*.*proj", SearchOption.AllDirectories)
                                .Where(pf => !projectExtensionExclusions.Any(ex => pf.EndsWith(ex)));

            foreach (String projectFile in projectFiles)
            {
                Project project = _allProjects.SingleOrDefault(p => p.PathAndName == projectFile);

                if (project == null)
                {
                    project = new Project
                    {
                        PathAndName = projectFile,
                        Path = Path.GetDirectoryName(projectFile),
                        Name = Path.GetFileNameWithoutExtension(projectFile)
                    };

                    _allProjects.Add(project);
                }

                solution.Projects.Add(project);

                LoadAllPackagesConfigForProject(project);
            }
        }

        private void LoadAllPackagesConfigForProject(Project project)
        {
            IEnumerable<String> packagesConfigFiles = Directory.GetFiles(project.Path, "packages.config", SearchOption.AllDirectories)
                .Where(pc => !pc.Contains(".nuget"));

            foreach (string packageConfigFile in packagesConfigFiles)
            {
                IEnumerable<XElement> packageReferences = XDocument.Load(packageConfigFile).Descendants("package");

                foreach (var packageReference in packageReferences)
                {
                    string name = packageReference.Attribute("id").Value;
                    string version = packageReference.Attribute("version").Value;

                    Package package = _allPackages.SingleOrDefault(p => p.IsEqual(name, version));

                    if (package == null)
                    {
                        package = new Package(name, version);
                        _allPackages.Add(package);
                    }

                    project.Packages.Add(package);
                }
            }
        }

        private void LoadAllLibrariesForSolutions()
        {
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            foreach (Solution solution in _allSolutions)
            {
                foreach (Project project in solution.Projects)
                {
                    // Get all projects, local libraries and GAC references
                    XDocument projectDoc = XDocument.Load(project.PathAndName);
                    IEnumerable<XElement> projectReferences = projectDoc.Descendants(ns + "ProjectReference");

                    //References between projects of the same solution
                    foreach (var projectReference in projectReferences)
                    {
                        string projectReferenceName = projectReference.Element(ns + "Name").Value;
                        var prj = solution.Projects.SingleOrDefault(p => p.Name == projectReferenceName);
                        if (prj != null)
                            project.Projects.Add(prj);
                        else
                            Console.WriteLine(String.Format("Project Reference: {0}, project not found in: {1}", projectReferenceName, project.Path));
                    }

                    //Local libraries and GAC references
                    IEnumerable<XElement> references = projectDoc.Descendants(ns + "Reference").Where(r => !r.Value.Contains(@"\packages\"));
                    foreach (XElement reference in references)
                    {
                        string name = reference.Attribute("Include").Value;
                        bool isGAC = !reference.Elements(ns + "HintPath").Any();

                        var library = _allLibraries.SingleOrDefault(l => l.Name == name);

                        if (library == null)
                        {
                            library = new Library { Name = name, IsGAC = isGAC };
                            project.Libraries.Add(library);
                            _allLibraries.Add(library);
                        }
                    }
                }
            }
        }


        private void GenerateDGML(string filename)
        {
            DGMLWriter dgml = new DGMLWriter();
            AddProjectsNodes(dgml);
            AddPackagesNodes(dgml);
            AddLibrariesNodes(dgml);
            AddLinksBetweenNodes(dgml);
            AddStyleToGraph(dgml);

            dgml.Serialize(filename);
        }

        private void AddPackagesNodes(DGMLWriter dgml)
        {
            foreach (Package package in _allPackages)
            {
                dgml.AddNode(package.Id, null, null, "Package");
            }

            AddGroupsForDiferentsVersionOfPackages(dgml);
        }

        private void AddProjectsNodes(DGMLWriter dgml)
        {
            foreach (Project project in _allProjects)
            {
                dgml.AddNode(project.Name, null, null, "Project");
            }
        }

        private void AddLibrariesNodes(DGMLWriter dgml)
        {
            dgml.AddNode("GlobalAssemblyCache", "Global Assembly Cache", "Collapsed", "GAC Library");
            dgml.AddNode("LocalLibraries", "Local Libraries", "Expanded", "Libraries");

            foreach (Library library in _allLibraries)
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

        private void AddGroupsForDiferentsVersionOfPackages(DGMLWriter dgml)
        {
            var groupedPackages = _allPackages
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

        private void AddLinksBetweenNodes(DGMLWriter dgml)
        {
            foreach (Solution solution in _allSolutions)
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
