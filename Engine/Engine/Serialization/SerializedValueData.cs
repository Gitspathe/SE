using Newtonsoft.Json;
using System;

namespace SE.Serialization
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SerializedValueData
    {
        public dynamic Value { get; set; }
        public int? ValueConverter { get; set; }
        [JsonIgnore] public Type Type { get; set; }
        public bool Override { get; set; }

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
