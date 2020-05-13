using System;
using DeeZ.Core.Extensions;
using DeeZ.Engine.Utility;
using FastMember;

namespace SE.Serialization
{
    public abstract class EngineSerializerBase
    {
        public QuickList<SerializedValue> ValueWrappers { get; } = new QuickList<SerializedValue>();
        public TypeAccessor Accessor { get; protected set; }

        protected bool initialized;

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
            return data.Serialize();
        }

        public void Deserialize(string jsonData)
        {
            if (!initialized)
                throw new InvalidOperationException("Not initialized!");

            Restore(jsonData.Deserialize<EngineSerializerData>());
        }

        public void Restore(EngineSerializerData data)
        {
            if (!initialized)
               throw new InvalidOperationException("Not initialized!");

            for (int i = 0; i < ValueWrappers.Count; i++) {
                ValueWrappers.Array[i].Restore(data.ValueData[i]);
            }
        }

        public EngineSerializerBase()
        {
            initialized = false;
        }
    }
}
