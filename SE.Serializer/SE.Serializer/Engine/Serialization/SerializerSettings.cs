using SE.Core;
using SE.Serialization.Resolvers;
using SE.Serialization.Attributes;

namespace SE.Serialization
{
    /// <summary>
    /// Settings used to control how the serializer behaves.
    /// </summary>
    public sealed class SerializerSettings
    {
        /// <summary>Which format to use.</summary>
        public Formatting Formatting = Formatting.Text;
        /// <summary>How the serializer handles null values.</summary>
        public NullValueHandling NullValueHandling = NullValueHandling.Ignore;
        /// <summary>How the serializer handles default values.</summary>
        public DefaultValueHandling DefaultValueHandling = DefaultValueHandling.Serialize;
        /// <summary>How the serializer converts data. Determines performance and parsing error resilience.</summary>
        public ConvertBehaviour ConvertBehaviour = ConvertBehaviour.NameAndOrder;
        /// <summary>How the serializer handles types.</summary>
        public TypeHandling TypeHandling = TypeHandling.Auto;
        /// <summary>How verbose the serialized Type information is.</summary>
        public TypeNaming TypeNaming = TypeNaming.Minimal;
        /// <summary>How the serializer behaves when encountering a recursive loop of references.</summary>
        public ReferenceLoopHandling ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        /// <summary>TODO comment</summary>
        public ReferenceHandling ReferenceHandling = ReferenceHandling.Preserve;
        /// <summary>How many levels deep the serializer will process.</summary>
        public int MaxDepth = 10;

        // TODO: HEADER!!
        /// <summary>Whether or not to handle the header. Allows for seamless conversion between binary and text formatting.</summary>
        public bool UseHeader = true;

        /// <summary>Which resolver is used to determine the Converters used for given types.</summary>
        public ConverterResolver Resolver = Serializer.DefaultResolver;
    }

    /// <summary>
    /// Controls how the serializer handles null values.
    /// </summary>
    public enum NullValueHandling
    {
        /// <summary>Null values are ignored.</summary>
        Ignore,

        /// <summary>When a null value is encountered, the value is set to default.</summary>
        DefaultValue
    }

    /// <summary>
    /// Controls how generated serializers behave when converting data from and to bytes.
    /// </summary>
    public enum ConvertBehaviour
    {
        /// <summary>Only declaration order is stored with the data. This options is fast but has lower resilience to parsing errors.
        ///          It's recommended to use <see cref="SerializeAttribute"/> on variables with this setting.</summary>
        Order,

        /// <summary>Declaration order and name is stored with the data. Both are used during deserialization.
        ///          This option is the slowest, but has the highest resilience to parsing errors.</summary>
        NameAndOrder,

        /// <summary>Intended for human-readable configuration files - ONLY the name is stored with the data. 
        ///          Cannot be used with binary. Has a low resilience to errors, as the order may not be preserved.</summary>
        Configuration
    }

    /// <summary>
    /// Controls how the serializer handles default values.
    /// </summary>
    public enum DefaultValueHandling
    {
        /// <summary>Default values are not serialized.</summary>
        Ignore,

        /// <summary>Default values are serialized.</summary>
        Serialize
    }

    /// <summary>
    /// Controls how the serializer behaves when dealing with types.
    /// </summary>
    public enum TypeHandling
    {
        /// <summary>Type information is not serialized. Instantiated object types are inferred from the original
        ///          field/property type when possible. Faster and uses less space.</summary>
        Ignore,

        /// <summary>Types are serialized into strings when an object's type does not match it's field, 
        ///          such as when the object is polymorphic. Instantiated object types are inferred from the original 
        ///          field/property type OR serialized Type strings. Slower and uses more space.</summary>
        Auto,

        /// <summary>Types are always serialized into strings. Instantiated object types are restored from serialized
        ///          Type strings. Slowest and uses the most space. <see cref="Auto"/> is preferred.</summary>
        Always
    }

    /// <summary>
    /// How the serializer behaves when encountering a recursive loop of references.
    /// </summary>
    public enum ReferenceLoopHandling
    {
        /// <summary>Reference loops are not serialized nor deserialized.</summary>
        Ignore,

        /// <summary>Reference loops will throw an error when detected.</summary>
        Error
    }

    public enum ReferenceHandling
    {
        Ignore,

        Preserve
    }

    /// <summary>
    /// Which format to use for serialization.
    /// </summary>
    public enum Formatting
    {
        /// <summary>Data is written in raw binary. Faster and saves space, but is not human-readable.</summary>
        Binary,

        /// <summary>Data is written in a YAML-like UTF-8 format. Slower and requires more space, but is human-readable.</summary>
        Text
    }

    /// <summary>
    /// How much information the serializer includes when writing Type information.
    /// </summary>
    public enum TypeNaming
    {
        /// <summary>Minimum info - type and assembly name.</summary>
        Minimal,

        /// <summary>Full info - type and assembly name, public key token.</summary>
        Full
    }
}
