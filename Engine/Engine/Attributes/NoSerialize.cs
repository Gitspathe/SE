using System;

namespace SE.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class NoSerializeAttribute : Attribute { }
}
