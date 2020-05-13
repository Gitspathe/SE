using System;
using Newtonsoft.Json;

namespace SE.Serialization
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SerializedValueData
    {
        public dynamic Value;
        public int? ValueConverter;
        [JsonIgnore] public Type Type;
        public bool Override;

        public void Setup(dynamic value, bool overRide, Type type)
        {
            Value = value;
            Override = overRide;
            Type = type;
            if (SerializerReflection.TypeTable.TryGetKey(Type, out int typeID)) {
                ValueConverter = typeID;
            }
        }

        public SerializedValueData(SerializedValue valueBase)
        {
            Setup(valueBase.Value, valueBase.Override, valueBase.Value.GetType());
        }

        public SerializedValueData() { }
    }
}
