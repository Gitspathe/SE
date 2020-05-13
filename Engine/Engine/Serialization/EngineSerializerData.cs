using DeeZ.Engine.Utility;
using Newtonsoft.Json;

namespace SE.Serialization
{
    [JsonObject(MemberSerialization.OptOut)]
    public class EngineSerializerData
    {
        public SerializedValueData[] ValueData;

        public EngineSerializerData(QuickList<SerializedValue> serializerData)
        {
            SerializedValueData[] values = new SerializedValueData[serializerData.Count];
            for (int i = 0; i < serializerData.Count; i++) {
                values[i] = new SerializedValueData(serializerData.Array[i]);
            }
            ValueData = values;
        }

        public EngineSerializerData() { }
    }
}
