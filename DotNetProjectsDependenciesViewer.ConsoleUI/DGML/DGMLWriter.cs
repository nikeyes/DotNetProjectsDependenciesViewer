using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DotNetProjectsDependenciesViewer.ConsoleUI.DGML
{
    public class DGMLWriter
    {
        public List<Node> Nodes { get; protected set; }
        public List<Link> Links { get; protected set; }
        public List<Style> Styles { get; protected set; }

        public DGMLWriter()
        {
            Nodes = new List<Node>();
            Links = new List<Link>();
            Styles = new List<Style>();

        }

        public void AddNode(string id, string label, string group, string category)
        {
            this.Nodes.Add(new Node(id, label, group, category));
        }

        public void AddLink(string source, string target, string label, string category)
        {
            this.Links.Add(new Link(source, target, label, category));
        }

        public void AddStyle(string groupLabel, string color)
        {
            this.Styles.Add(new Style(groupLabel, color));
        }

        public void Serialize(string xmlpath)
        {
            Graph g = new Graph();
            g.Nodes = this.Nodes.ToArray();
            g.Links = this.Links.ToArray();
            g.Styles = this.Styles.ToArray();

            XmlRootAttribute root = new XmlRootAttribute("DirectedGraph");
            root.Namespace = "http://schemas.microsoft.com/vs/2009/dgml";
            XmlSerializer serializer = new XmlSerializer(typeof(Graph), root);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xmlWriter = XmlWriter.Create(xmlpath, settings);
            serializer.Serialize(xmlWriter, g);
        }
    }
}
