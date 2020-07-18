using System;

namespace SE.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeAttribute : Attribute
    {
        public string Name;
        public ushort? Order;

        public SerializeAttribute() { }

        public SerializeAttribute(string name)
        {
            Name = name;
        }

        public SerializeAttribute(string name, ushort order)
        {
            Name = name;
            Order = order;
        }

        public SerializeAttribute(ushort order, string name = null)
        {
            Name = name;
            Order = order;
        }
    }
}
