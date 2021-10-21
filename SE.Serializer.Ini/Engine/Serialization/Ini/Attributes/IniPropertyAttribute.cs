using System;

namespace SE.Serialization.Ini.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IniPropertyAttribute : Attribute
    {
        public readonly int Order;
        public readonly string Comment;

        public IniPropertyAttribute(int order, string comment)
        {
            Order = order;
            Comment = comment;
        }
    }
}
