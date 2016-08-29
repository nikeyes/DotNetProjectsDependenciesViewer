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

            string path = Path.Combine(rootFolder, "Dependencies.dgml");
            DGMLPackageDependencies packageDependencies = new DGMLPackageDependencies();

            packageDependencies.generateDGML(path, _allSolutions, _allProjects, _allPackages, _allLibraries);
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
                
                        IEnumerable<Project> referencesProjects = solution.Projects.Where(p => p.Name == projectReferenceName);

                        if (referencesProjects.Count() > 1)
                        {
                            Console.WriteLine(String.Format("[ERROR] Project Reference: {0}. Maybe you may have projects (.csproj) in folder {1}  that are not in solution file {2}", projectReferenceName, solution.Path, solution.PathAndName));
                            foreach (Project p in referencesProjects)
                            {
                                Console.WriteLine(String.Format(p.PathAndName));
                            }
                        }
                        else
                        { 
                                if (referencesProjects.Any())
                                {
                                    Project prj = referencesProjects.First();
                                    project.Projects.Add(prj);
                                }
                                else
                                {
                                    Console.WriteLine(String.Format("[WARNING] Project Reference: {0}, project not found in: {1}", projectReferenceName, project.Path));
                                }
                        }
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
    }
}
