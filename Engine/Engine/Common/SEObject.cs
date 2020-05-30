using SE.Serialization;

namespace SE.Common
{
    public class SEObject : ISerializedObject
    {
        public ulong InstanceID { get; internal set; }
    }
}
