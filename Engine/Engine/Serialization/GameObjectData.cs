using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace SE.Serialization
{
    [JsonObject(MemberSerialization.OptOut)]
    public class GameObjectData
    {
        public string EngineName;
        public Type Type;

        public Vector2 Position;
        public Vector2 Scale;
        public float Rotation;

        public List<ComponentData> componentData;
    }
}
