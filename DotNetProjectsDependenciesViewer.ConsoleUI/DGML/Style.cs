using System.Xml.Serialization;

namespace DotNetProjectsDependenciesViewer.ConsoleUI.DGML
{
    public class Style
    {
        [XmlAttribute]
        public string TargetType;
        [XmlAttribute]
        public string GroupLabel;
        [XmlAttribute]
        public string ValueLabel;

        public Condition Condition { get; set; }
        public Setter Setter { get; set; }

        public Style()
        {
        }

        public Style(string groupLabel, string color)
        {
            this.TargetType = "Node";
            this.GroupLabel = groupLabel;
            this.ValueLabel = "True";

            Condition = new Condition(GroupLabel);
            Setter = new Setter(color);
        }
    }
}
