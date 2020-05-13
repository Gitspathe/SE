using System;
using Newtonsoft.Json;

namespace SE.Serialization
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ComponentData
    {
        public ulong ComponentIndex;
        public EngineSerializerData AdditionalData;
        public Type Type;
    }
}
