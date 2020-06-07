using System;
using FastMember;
using SE.Core;
using SE.Core.Extensions;
using SE.Utility;

namespace SE.Serialization
{
    public abstract class EngineSerializerBase : IDisposable
    {
        public PooledList<SerializedValue> ValueWrappers { get; } = new PooledList<SerializedValue>(Config.Performance.UseArrayPoolCore);
        public TypeAccessor Accessor { get; protected set; }

        protected bool initialized;
        private bool isDisposed;

        protected void Hook(object obj, string name)
        {
            ValueWrappers.Add(new SerializedValue(Accessor, obj, name));
        }

        protected abstract void Setup();

        public void Initialize()
        {
            Setup();
            initialized = true;
        }

        public string Serialize()
        {
            if (!initialized)
                throw new InvalidOperationException("Not initialized!");

            EngineSerializerData data = new EngineSerializerData(ValueWrappers);
            string str = data.Serialize();
            data.Dispose();
            return str;
        }

        public void Deserialize(string jsonData)
        {
            if (!initialized)
                throw new InvalidOperationException("Not initialized!");

            EngineSerializerData data = jsonData.Deserialize<EngineSerializerData>();
            Restore(data);
            data.Dispose();
        }

        public void Restore(EngineSerializerData data)
        {
            if (!initialized)
               throw new InvalidOperationException("Not initialized!");

            for (int i = 0; i < ValueWrappers.Count; i++) {
                ValueWrappers.Array[i].Restore(data.ValueData.Array[i]);
            }
        }

        public EngineSerializerBase()
        {
            initialized = false;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing = true)
        {
            if(isDisposed)
                return;

            ValueWrappers?.Dispose();
            isDisposed = true;
        }
    }
}
