using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            /*
            var graph = new XElement(dgmlns + "DirectedGraph", new XAttribute("GraphDirection", "LeftToRight"),
                new XElement(dgmlns + "Nodes",
                    this.projects.Select(p => CreateNode(p.Name, "Project")),
                    this.libraries.Select(l => CreateNode(l.Name, l.IsGAC ? "GAC Library" : "Library", l.Name.Split(',')[0])),
                    this.packages.Select(p => CreateNode(p.Name + " " + p.Version, "Package")),
                    CreateNode("AllProjects", "Project", label: "All Projects", @group: "Expanded"),
                    CreateNode("AllPackages", "Package", label: "All Packages", @group: "Expanded"),
                    CreateNode("LocalLibraries", "Library", label: "Local Libraries", @group: "Expanded"),
                    CreateNode("GlobalAssemblyCache", "GAC Library", label: "Global Assembly Cache", @group: "Collapsed")),
                new XElement(dgmlns + "Links",
                    this.projects.SelectMany(p => p.Projects.Select(pr => new { Source = p, Target = pr }))
                        .Select(l => CreateLink(l.Source.Name, l.Target.Name, "Project Reference")),
                    this.projects.SelectMany(p => p.Libraries.Select(l => new { Source = p, Target = l }))
                        .Select(l => CreateLink(l.Source.Name, l.Target.Name, "Library Reference")),
                    this.projects.SelectMany(p => p.Packages.Select(pa => new { Source = p, Target = pa }))
                        .Select(l => CreateLink(l.Source.Name, l.Target.Name + " " + l.Target.Version, "Installed Package")),
                    this.projects.Select(p => CreateLink("AllProjects", p.Name, "Contains")),
                    this.packages.Select(p => CreateLink("AllPackages", p.Name + " " + p.Version, "Contains")),
                    this.libraries.Where(l => !l.IsGAC).Select(l => CreateLink("LocalLibraries", l.Name, "Contains")),
                    this.libraries.Where(l => l.IsGAC).Select(l => CreateLink("GlobalAssemblyCache", l.Name, "Contains"))),
                // No need to declare Categories, auto generated
                new XElement(dgmlns + "Styles",
                    CreateStyle("Project", "Blue"),
                    CreateStyle("Package", "Purple"),
                    CreateStyle("Library", "Green"),
                    CreateStyle("GAC Library", "LightGreen")));
            */

            //var doc = new XDocument(graph);
            //doc.Save(filename);
        }

        private void AddPackagesNodes(DGMLWriter dgml)
        {
            foreach (Package package in _allPackages)
            {
                dgml.AddNode(new DGMLWriter.Node(package.Id, null, null, "Package"));
            }

            AddGroupsForDiferentsVersionOfPackages(dgml);
        }

        private void AddProjectsNodes(DGMLWriter dgml)
        {
            foreach (Project project in _allProjects)
            {
                dgml.AddNode(new DGMLWriter.Node(project.Name, null, null, "Project"));
            }
        }

        private void AddLibrariesNodes(DGMLWriter dgml)
        {
            dgml.AddNode(new DGMLWriter.Node("GlobalAssemblyCache", "Global Assembly Cache", "Collapsed", "GAC Library"));
            dgml.AddNode(new DGMLWriter.Node("LocalLibraries", "Local Libraries", "Expanded", "Libraries"));

            foreach (Library library in _allLibraries)
            {
                if (library.IsGAC)
                {
                    dgml.AddNode(new DGMLWriter.Node(library.Name, null, null, "GAC Library"));
                    dgml.AddLink(new DGMLWriter.Link("GlobalAssemblyCache", library.Name, null, "Contains"));
                }
                else
                {
                    dgml.AddNode(new DGMLWriter.Node(library.Name, null, null, "Library"));
                    dgml.AddLink(new DGMLWriter.Link("LocalLibraries", library.Name, null, "Contains"));
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
                dgml.AddNode(new DGMLWriter.Node(packageGrouped[0].Name, packageGrouped[0].Name, "Expanded", "PackageGroup"));

                foreach (Package package in packageGrouped)
                {
                    dgml.AddLink(new DGMLWriter.Link(package.Name, package.Id, null, "Contains"));
                }
                //dgml.AddLink(new DGMLWriter.Link(project.Name, package.Name + " " + package.Version, null, "Installed Package"));
            }
        }

        private void AddLinksBetweenNodes(DGMLWriter dgml)
        {
            foreach (Solution solution in _allSolutions)
            {
                dgml.AddNode(new DGMLWriter.Node(solution.Name, solution.Name, "Expanded", "Solution"));

                foreach (Project project in solution.Projects)
                {
                    dgml.AddLink(new DGMLWriter.Link(solution.Name, project.Name, null, "Contains"));

                    foreach (Project projectChild in project.Projects)
                    {
                        dgml.AddLink(new DGMLWriter.Link(project.Name, projectChild.Name, null, "Project Reference"));
                    }

                    foreach (Package package in project.Packages)
                    {
                        dgml.AddLink(new DGMLWriter.Link(project.Name, package.Id, null, "Installed Package"));
                    }

                    foreach (Library library in project.Libraries)
                    {
                        dgml.AddLink(new DGMLWriter.Link(project.Name, library.Name, null, "Library Reference"));
                    }
                }
            }
        }

        private static void AddStyleToGraph(DGMLWriter dgml)
        {
            dgml.AddStyle(new DGMLWriter.Style("Project", "Blue"));
            dgml.AddStyle(new DGMLWriter.Style("Package", "Purple"));
            dgml.AddStyle(new DGMLWriter.Style("PackageGroup", "Plum"));
            dgml.AddStyle(new DGMLWriter.Style("Library", "Green"));
            dgml.AddStyle(new DGMLWriter.Style("GAC Library", "LightGreen"));
        }
    }
}
