using LiteNetLib.Utils;
using Vector2 = System.Numerics.Vector2;

namespace SE.Engine.Networking
{
    public static class LiteNetLibExtensions
    {
        public static Vector2 GetVector2(this NetDataReader reader)
            => new Vector2(reader.GetFloat(), reader.GetFloat());

        public static void PutVector2(this NetDataWriter write, Vector2 val)
        {
            write.Put(val.X);
            write.Put(val.Y);
        }
    }
}
