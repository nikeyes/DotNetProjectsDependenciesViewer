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

            var optionSet = new OptionSet() {
                { "p|path=", "the root path of the solutions to search dependencies",
                  p => rootFolder = p},
                {"nuget=", "nuget packages to show only its dependencies. You can use as many times as you want this parameter.",
                    n => nugetPackagesToFilter.Add(n) },
                { "h|help",  "show this message and exit",
                  v => showHelp = v != null }
            };

            List<string> parameters;
            try
            {
                parameters = optionSet.Parse(args);
                if (String.IsNullOrWhiteSpace(rootFolder))
                {
                    throw new OptionException("Missing required option -p=path", "p|path=");
                }
                else if (Directory.Exists(rootFolder) == false)
                {
                    throw new OptionException(String.Format("Invalid solutions path. Folder does not exist: {0}", rootFolder), "p|path=");
                }

            }
            catch (OptionException e)
            {
                Console.Write("DotNetProjectsDependenciesViewer: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `DotNetProjectsDependenciesViewer --help' for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(optionSet);
                return;
            }

            DependencyGenerator gen = new DependencyGenerator(rootFolder, nugetPackagesToFilter);

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
