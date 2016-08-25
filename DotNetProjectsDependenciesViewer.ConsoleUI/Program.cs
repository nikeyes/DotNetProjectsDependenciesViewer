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

            if (args.Length != 0)
            {
                rootFolder = args[0];
                DependencyGenerator gen = new DependencyGenerator(rootFolder);
            }
            else
            {
                Console.WriteLine("Please enter a path with .Net projects or solutions.");
            }

            
        }
    }
}
