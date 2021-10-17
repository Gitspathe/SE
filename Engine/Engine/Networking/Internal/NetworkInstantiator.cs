using SE.Engine.Networking;
using System;
using static SE.Core.Network;
using Vector2 = System.Numerics.Vector2;

namespace SE.Networking.Internal
{
    public static class NetHelper
    {
        public static Instantiator Spawner => (Instantiator)GetExtension<Instantiator>();

        public static void Instantiate(string type, string owner = "SERVER", Vector2? position = null)
            => Spawner.Instantiate(type, owner, new object[] { position ?? Vector2.Zero, 0f, Vector2.One });

        public static void Destroy(uint netID)
            => Spawner.Destroy(netID);

        public static void AddSpawnable(string key, Type type)
            => Spawner.AddSpawnable(key, type);
    }
}
