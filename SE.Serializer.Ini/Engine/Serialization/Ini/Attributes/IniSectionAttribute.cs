using System;

namespace SE.Serialization.Ini.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IniSectionAttribute : Attribute
    {
        public readonly string Name;

        public IniSectionAttribute(string name)
        {
            Name = name;
        }
    }
}
