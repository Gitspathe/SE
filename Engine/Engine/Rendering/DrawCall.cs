using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SE.Rendering
{
    public readonly struct DrawCall : IEquatable<DrawCall>
    {
        public readonly Texture2D Texture;
        public readonly Effect Effect;

        public DrawCall(Texture2D texture, Effect effect)
        {
            Texture = texture;
            Effect = effect;
        }

        public static bool operator ==(DrawCall a, DrawCall b) => a.Equals(b);
        public static bool operator !=(DrawCall a, DrawCall b) => !(a == b);

        public bool Equals(DrawCall other)
            => Texture == other.Texture && Effect == other.Effect;

        public override bool Equals(object obj)
            => obj is DrawCall other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Texture, Effect);

        public static DrawCall Empty => new DrawCall(null, null);
    }

    public struct DrawCallComparer : IEqualityComparer<DrawCall>
    {
        public bool Equals(DrawCall x, DrawCall y) => x.Texture == y.Texture && x.Effect == y.Effect;
        public int GetHashCode(DrawCall obj) => obj.GetHashCode();
    }
}
