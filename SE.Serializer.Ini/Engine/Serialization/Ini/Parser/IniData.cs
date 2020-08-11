using SE.Utility;
using System.Collections.Generic;
using System.Xml.Schema;

namespace SE.Serialization.Ini.Parser
{
    public interface ICloneable<out T>
    {
        T Clone();
    }

    public class IniData : ICloneable<IniData>
    {
        private QuickList<IniSection> sections = new QuickList<IniSection>();
        private Dictionary<string, IniSection> sectionLookup = new Dictionary<string, IniSection>();

        public IniData() { }

        public IniData(QuickList<IniSection> sections)
        {
            this.sections = sections;
            for (int i = 0; i < sections.Count; i++) {
                IniSection section = sections.Array[i];
                sectionLookup.Add(section.Name, section);
                section.ParentData = this;
            }
        }

        public IniSection this[string key] => sectionLookup[key];

        public void AddSection(IniSection section)
        {
            if (sectionLookup.ContainsKey(section.Name))
                RemoveSection(section.Name);

            sections.Add(section);
            sectionLookup.Add(section.Name, section);
            section.ParentData = this;
        }

        public void RemoveSection(string name)
        {
            if(!sectionLookup.Remove(name))
                return;

            for (int i = sections.Count - 1; i > 0; i--) {
                if (sections.Array[i].Name == name) {
                    sections.Array[i].ParentData = null;
                    sections.RemoveAt(i);
                }
            }
        }

        public void RemoveSection(IniSection section)
        {
            if(!sectionLookup.Remove(section.Name))
                return;

            for (int i = sections.Count - 1; i > 0; i--) {
                if (sections.Array[i] == section) {
                    section.ParentData = null;
                    sections.RemoveAt(i);
                }
            }
        }

        public IniData Clone() 
            => new IniData(DataHelper.CloneList(sections));
    }

    public class IniSection : ICloneable<IniSection>
    {
        public string Name {
            get => name;
            set {
                ParentData.RemoveSection(this);
                name = value;
                ParentData.AddSection(this);
            }
        }
        private string name = "";

        internal IniData ParentData;

        private QuickList<IniNode> nodes = new QuickList<IniNode>();
        private Dictionary<string, IniNode> nodeLookup = new Dictionary<string, IniNode>();

        public QuickList<string> Comments { get; } = new QuickList<string>();

        public IniSection() { }

        public IniSection(string name, QuickList<string> comments, QuickList<IniNode> nodes)
        {
            this.name = name ?? "";
            Comments = comments ?? new QuickList<string>();
            this.nodes = nodes ?? new QuickList<IniNode>();
            for (int i = 0; i < this.nodes.Count; i++) {
                IniNode node = this.nodes.Array[i];
                nodeLookup.Add(node.Name, node);
                node.ParentSection = this;
            }
        }

        public IniNode this[string key] => nodeLookup[key];

        public void AddNode(IniNode node)
        {
            if (nodeLookup.ContainsKey(node.Name))
                RemoveNode(node.Name);

            nodes.Add(node);
            nodeLookup.Add(node.Name, node);
            node.ParentSection = this;
        }

        public void RemoveNode(string node)
        {
            if(!nodeLookup.Remove(node))
                return;

            for (int i = nodes.Count - 1; i > 0; i--) {
                if (nodes.Array[i].Name == name) {
                    nodes.Array[i].ParentSection = null;
                    nodes.RemoveAt(i);
                }
            }
        }

        public void RemoveNode(IniNode node)
        {
            if(!nodeLookup.Remove(node.Name))
                return;
            
            for (int i = nodes.Count - 1; i > 0; i--) {
                if (nodes.Array[i] == node) {
                    node.ParentSection = null;
                    nodes.RemoveAt(i);
                }
            }
        }

        public IniSection Clone()
            => new IniSection(Name, Comments, DataHelper.CloneList(nodes));
    }

    public class IniNode : ICloneable<IniNode>
    {
        public string Name {
            get => name;
            set {
                ParentSection.RemoveNode(this);
                name = value;
                ParentSection.AddNode(this);
            }
        }
        private string name;

        public QuickList<string> Values;

        internal IniSection ParentSection;

        public QuickList<string> Comments { get; } = new QuickList<string>();
        
        public IniNode() { }

        public IniNode(string name, QuickList<string> value, QuickList<string> comments)
        {
            this.name = name;
            Values = value ?? new QuickList<string>();
            Comments = comments ?? new QuickList<string>();
        }

        public IniNode Clone() 
            => new IniNode(Name, Values.Copy(), Comments.Copy());
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
