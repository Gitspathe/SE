using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;
using SE.Utility;

namespace SE.Serialization
{
    public class GameObjectData : IDisposable
    {
        public string EngineName { get; set; }
        [IgnoreDataMember] public Type Type { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Scale { get; set; }
        public float Rotation { get; set; }

        public PooledList<ComponentData> componentData { get; set; }

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
