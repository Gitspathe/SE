using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using SE.Utility;

namespace SE.Serialization
{
    [JsonObject(MemberSerialization.OptOut)]
    public class GameObjectData : IDisposable
    {
        public string EngineName;
        public Type Type;

        public Vector2 Position;
        public Vector2 Scale;
        public float Rotation;

        public PooledList<ComponentData> componentData;

        private bool isDisposed;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing = true)
        {
            if(isDisposed)
                return;

            componentData?.Dispose();
            isDisposed = true;
        }
    }
}
