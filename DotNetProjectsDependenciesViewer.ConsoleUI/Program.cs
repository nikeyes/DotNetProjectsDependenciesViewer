using DotNetProjectsDependenciesViewer.ConsoleUI.ClassicASP;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DotNetProjectsDependenciesViewer.ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootFolder = string.Empty;
            bool showHelp = false;
            List<string> nugetPackagesToFilter = new List<string>();
            string tecnology = String.Empty;

            var optionSet = new OptionSet() {
                { "p|path=", "REQUIRED - the root path of the solutions to search dependencies. Sample: -p=d:",
                  p => rootFolder = p},
                { "n|nuget=", "nuget packages to show only its dependencies. You can use as many times as you want this parameter. Sample: -n=newtonsoft.json -n=Microsoft.AspNet.Razor-3.2-3",
                  n => nugetPackagesToFilter.Add(n)},
                { "t|tecnology=", "tecnology to analyze (classicASP, .Net)",
                  t => tecnology = t},
                { "h|help",  "show this message and exit",
                  v => showHelp = v != null }
            };

            List<string> parameters;
            try
            {
                parameters = optionSet.Parse(args);

                if (showHelp)
                {
                    ShowHelp(optionSet);
                    return;
                }

                if (String.IsNullOrWhiteSpace(rootFolder))
                {
                    throw new OptionException("Missing required option -p=path", "p|path=");
                }
                else if (Directory.Exists(rootFolder) == false)
                {
                    throw new OptionException(String.Format("Invalid solutions path. Folder does not exist: {0}", rootFolder), "p|path=");
                }

                if (String.IsNullOrWhiteSpace(tecnology))
                {
                    tecnology = ".Net";
                }

                if (tecnology != ".Net" && tecnology != "classicASP")
                {
                    throw new OptionException("Not Supported Tecnology: Only Support .Net or classicASP", "t|tecnology=");
                }

            }
            catch (OptionException e)
            {
                Console.Write("DotNetProjectsDependenciesViewer: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `DotNetProjectsDependenciesViewer --help' for more information.");
                return;
            }

            if (tecnology == ".Net")
            {
                DependencyGenerator gen = new DependencyGenerator(rootFolder, nugetPackagesToFilter);
            }
            else
            {
                DependencyGeneratorClassicASP gen = new DependencyGeneratorClassicASP(rootFolder);
            }
            
            Console.WriteLine(String.Empty);
            Console.WriteLine(String.Format("Go to {0} and open Dependencies.dgml with Visual Studio.", rootFolder));

        }

        static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine("Usage: DotNetProjectsDependenciesViewer [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
        }

    }
}
