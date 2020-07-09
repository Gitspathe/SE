using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Engine.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeAttribute : Attribute
    {
        public string Name;
        public uint? Order;

        public SerializeAttribute() { }

        public SerializeAttribute(string name)
        {
            Name = name;
        }

        public SerializeAttribute(string name, uint order)
        {
            Name = name;
            Order = order;
        }

        public SerializeAttribute(uint order, string name = null)
        {
            Name = name;
            Order = order;
        }
    }
}
