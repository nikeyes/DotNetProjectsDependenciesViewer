using System.Xml.Serialization;

namespace DotNetProjectsDependenciesViewer.ConsoleUI.DGML
{
    public class Node
    {
        [XmlAttribute]
        public string Id;
        [XmlAttribute]
        public string Label;
        [XmlAttribute]
        public string Group;
        [XmlAttribute]
        public string Category;

        public Node()
        {
        }

        public Node(string id, string label, string group, string category)
        {
            this.Id = id;
            this.Label = label;
            this.Category = category;
            this.Group = group;
        }

       
    }
}
