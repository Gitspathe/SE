using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Engine.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializeIgnoreAttribute : Attribute
    {
    }
}
