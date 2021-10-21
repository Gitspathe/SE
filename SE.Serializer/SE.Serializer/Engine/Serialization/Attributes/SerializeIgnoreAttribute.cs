using System;

namespace SE.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeIgnoreAttribute : Attribute
    {
    }
}
