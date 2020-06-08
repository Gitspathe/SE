using System;
using Newtonsoft.Json;

namespace SE.Serialization
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ComponentData : IDisposable
    {
        public ulong ComponentIndex;

        public EngineSerializerData AdditionalData {
            get => additionalData;
            set {
                if (additionalData != null || (value == null && additionalData != null))
                    additionalData.Dispose();

                additionalData = value;
            }
        }
        private EngineSerializerData additionalData;

        public Type Type;

        private bool isDisposed;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing = true)
        {
            if(isDisposed)
                return;

            AdditionalData = null;
        }
    }
}
