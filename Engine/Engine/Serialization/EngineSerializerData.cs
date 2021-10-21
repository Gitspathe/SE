using Newtonsoft.Json;
using SE.Utility;
using System;

namespace SE.Serialization
{
    [JsonObject(MemberSerialization.OptOut)]
    public class EngineSerializerData : IDisposable
    {
        public QuickList<SerializedValueData> ValueData { get; set; } = new QuickList<SerializedValueData>();

        private bool isDisposed;

        public EngineSerializerData(QuickList<SerializedValue> serializerData)
        {
            for (int i = 0; i < serializerData.Count; i++) {
                ValueData.Add(new SerializedValueData(serializerData.Array[i]));
            }
        }

        public EngineSerializerData() { }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing = true)
        {
            if (isDisposed)
                return;

            isDisposed = true;
        }
    }
}
