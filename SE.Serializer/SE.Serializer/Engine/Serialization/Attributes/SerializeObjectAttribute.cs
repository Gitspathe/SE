using System;

namespace SE.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializeObjectAttribute : Attribute
    {
        public ObjectSerialization ObjectSerialization;

        public SerializeObjectAttribute(ObjectSerialization objectSerialization = ObjectSerialization.OptOut)
        {
            ObjectSerialization = objectSerialization;
        }
    }

    /// <summary>
    /// Controls which members are serialized.
    /// </summary>
    public enum ObjectSerialization
    {
        /// <summary>All public properties are serialized by default.</summary>
        OptOut,

        /// <summary>No fields or properties are serialized by default.</summary>
        OptIn,

        /// <summary>All fields and properties are serialized, including private ones, by default.</summary>
        Fields
    }
}
