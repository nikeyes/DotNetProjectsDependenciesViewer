using System.Xml.Serialization;

namespace DotNetProjectsDependenciesViewer.ConsoleUI.DGML
{
    public class Link
    {
        [XmlAttribute]
        public string Source;
        [XmlAttribute]
        public string Target;
        [XmlAttribute]
        public string Label;
        [XmlAttribute]
        public string Category;

        public Link()
        {
        }

        public Link(string source, string target, string label, string category)
        {
            this.Source = source;
            this.Target = target;
            this.Label = label;
            this.Category = category;
        }
    }
}
