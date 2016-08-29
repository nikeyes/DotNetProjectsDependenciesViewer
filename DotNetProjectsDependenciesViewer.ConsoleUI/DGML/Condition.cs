using System.Xml.Serialization;

namespace DotNetProjectsDependenciesViewer.ConsoleUI.DGML
{
    public class Condition
    {
        [XmlAttribute]
        public string Expression;

        public Condition()
        {
        }

        public Condition(string expression)
        {
            this.Expression = "HasCategory('" + expression + "')";
        }
    }
}
