using System.Xml.Serialization;

namespace DotNetProjectsDependenciesViewer.ConsoleUI.DGML
{
    public class Setter
    {
        [XmlAttribute]
        public string Property;
        [XmlAttribute]
        public string Value;

        public Setter()
        {
        }

        public Setter(string value)
        {
            this.Property = "Background";
            this.Value = value;
        }
    }
}
