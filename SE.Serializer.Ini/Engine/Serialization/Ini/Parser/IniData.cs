using SE.Utility;

namespace SE.Serialization.Ini.Parser
{
    public interface ICloneable<out T>
    {
        T Clone();
    }

    public class IniData : ICloneable<IniData>
    {
        public QuickList<IniSection> Sections { get; } = new QuickList<IniSection>();

        public IniData() { }

        public IniData(QuickList<IniSection> sections)
        {
            Sections = sections;
        }

        public IniData Clone() 
            => new IniData(DataHelper.CloneList(Sections));
    }

    public class IniSection : ICloneable<IniSection>
    {
        public string Name { get; set; }
        public QuickList<string> Comments { get; set; } = new QuickList<string>();
        public QuickList<IniNode> Nodes { get; set; } = new QuickList<IniNode>();
        
        public IniSection() { }

        public IniSection(string name, QuickList<string> comments, QuickList<IniNode> nodes)
        {
            Name = name;
            Comments = comments ?? new QuickList<string>();
            Nodes = nodes ?? new QuickList<IniNode>();
        }

        public IniSection Clone()
            => new IniSection(Name, Comments, DataHelper.CloneList(Nodes));
    }

    public class IniNode : ICloneable<IniNode>
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public QuickList<string> Comments { get; } = new QuickList<string>();
        
        public IniNode() { }

        public IniNode(string name, string value, QuickList<string> comments)
        {
            Name = name;
            Value = value;
            Comments = comments ?? new QuickList<string>();
        }

        public IniNode Clone() 
            => new IniNode(Name, Value, Comments.Copy());
    }

    internal static class DataHelper
    {
        public static QuickList<T> CloneList<T>(QuickList<T> list) where T : ICloneable<T>
        {
            QuickList<T> clone = new QuickList<T>(list.Count);
            for (int i = 0; i < list.Count; i++) {
                clone.Add(list.Array[i].Clone());
            }
            return clone;
        }
    }
}
