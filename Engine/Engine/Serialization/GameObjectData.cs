using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SE.Utility;

namespace SE.Serialization
{
    public class GameObjectData : IDisposable
    {
        public string EngineName { get; set; }
        [JsonIgnore] public Type Type { get; set; }

        public Vector3 Position { get; set; }
        public Vector2 Scale { get; set; }
        public float Rotation { get; set; }

        public QuickList<ComponentData> componentData { get; set; }

        private bool isDisposed;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing = true)
        {
            if(isDisposed)
                return;

            isDisposed = true;
        }
    }
}
