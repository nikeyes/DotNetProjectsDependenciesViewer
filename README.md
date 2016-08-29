# DotNetProjectsDependenciesViewer
Dependencies Viewer for .Net Projects and Solutions.
Generate a graph with the dependencies between solutions, projects, nuget packages, local bookstores and libraries GAC

## Usage:
DotNetProjectsDependenciesViewer.ConsoleUI.exe [OPTIONS]

**[OPTIONS]**

-p, --path=VALUE           REQUIRED - the root path of the solutions to search dependencies. Sample: -p=d:

-n, --nuget=VALUE          nuget packages to show only its dependencies. You can use as many times as you want this parameter.
                           Sample: -n=newtonsoft.json -n=Microsoft.AspNet.Razor-3.2-3

-h, --help                 show this message and exit

Samples: 

DotNetProjectsDependenciesViewer.ConsoleUI.exe **--help**

DotNetProjectsDependenciesViewer.ConsoleUI.exe **--path=d: -n=newtonsoft.json -n=Microsoft.AspNet.Razor-3.2-3 -n=Microsoft.AspNet.MVC**

## Result:
Generate a Dependencies.dgml in Path base .Net Projects. 
You can see this file with Visual Studio Community

## Result Samples:
### Simple Projects
![Alt text](/resources/SimpleProjects.PNG?raw=true "Simple Projects")

### Complex Projects
![Alt text](/resources/ComplexProjects.PNG?raw=true "Complex Projects")
