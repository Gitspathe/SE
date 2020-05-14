using SE.Engine.Networking;
using Vector2 = System.Numerics.Vector2;
using static SE.Core.Network;

namespace SE.Networking.Internal
{
    public static class NetHelper
    {
        public static void Instantiate(string type, string owner = "SERVER", Vector2? position = null)
        {
            Instantiator spawner = (Instantiator)GetExtension<Instantiator>();
            object[] p = { position ?? Vector2.Zero, 0f, Vector2.One };
            spawner.Instantiate(type, owner, p);
        }
    }
}
