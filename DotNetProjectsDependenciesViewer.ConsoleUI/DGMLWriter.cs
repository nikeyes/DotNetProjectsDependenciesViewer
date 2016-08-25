using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DotNetProjectsDependenciesViewer.ConsoleUI
{
    public class DGMLWriter
    {
        public struct Graph
        {
            public Node[] Nodes;
            public Link[] Links;
            public Style[] Styles;
        }

        public struct Node
        {
            [XmlAttribute]
            public string Id;
            [XmlAttribute]
            public string Label;
            [XmlAttribute]
            public string Group;
            [XmlAttribute]
            public string Category;

            public Node(string id, string label, string group, string category)
            {
                this.Id = id;
                this.Label = label;
                this.Category = category;
                this.Group = group;
            }
        }

        public struct Link
        {
            [XmlAttribute]
            public string Source;
            [XmlAttribute]
            public string Target;
            [XmlAttribute]
            public string Label;
            [XmlAttribute]
            public string Category;

            public Link(string source, string target, string label, string category)
            {
                this.Source = source;
                this.Target = target;
                this.Label = label;
                this.Category = category;
            }
        }

        public struct Style
        {
            [XmlAttribute]
            public string TargetType;
            [XmlAttribute]
            public string GroupLabel;
            [XmlAttribute]
            public string ValueLabel;

            public Condition Condition { get; set; }
            public Setter Setter { get; set; }


            public Style(string groupLabel, string color)
            {
                this.TargetType = "Node";
                this.GroupLabel = groupLabel;
                this.ValueLabel = "True";

                Condition = new DGMLWriter.Condition(GroupLabel);
                Setter = new DGMLWriter.Setter(color);
            }
        }

        public struct Condition
        {
            [XmlAttribute]
            public string Expression;

            public Condition(string expression)
            {
                this.Expression = "HasCategory('" + expression + "')";
            }
        }

        public struct Setter
        {
            [XmlAttribute]
            public string Property;
            [XmlAttribute]
            public string Value;

            public Setter(string value)
            {
                this.Property = "Background";
                this.Value = value;
            }
        }

        public List<Node> Nodes { get; protected set; }
        public List<Link> Links { get; protected set; }
        public List<Style> Styles { get; protected set; }

        public DGMLWriter()
        {
            Nodes = new List<Node>();
            Links = new List<Link>();
            Styles = new List<Style>();

        }

        public void AddNode(Node n)
        {
            this.Nodes.Add(n);
        }

        public void AddLink(Link l)
        {
            this.Links.Add(l);
        }

        public void AddStyle(Style g)
        {
            this.Styles.Add(g);
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
