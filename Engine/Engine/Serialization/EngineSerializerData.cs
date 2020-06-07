using Newtonsoft.Json;
using SE.Core;
using SE.Utility;
using System;
using System.Buffers;

namespace SE.Serialization
{
    [JsonObject(MemberSerialization.OptOut)]
    public class EngineSerializerData : IDisposable
    {
        public PooledList<SerializedValueData> ValueData = new PooledList<SerializedValueData>(Config.Performance.UseArrayPoolCore);

        private bool isDisposed;

        public EngineSerializerData(PooledList<SerializedValue> serializerData)
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
            if(isDisposed)
                return;

            ValueData.Dispose();
            isDisposed = true;
        }
    }
}
