using SE.Components;
using SE.Serialization;

namespace SEDemos
{
    public class SpriteBaseSerializer<T> : EngineSerializer<T> where T : SpriteBase
    {
        protected override void Setup()
        {
            Hook(Obj, nameof(Obj.LayerDepth));
            Hook(Obj, nameof(Obj.Color));
        }

        public SpriteBaseSerializer(T obj) : base(obj) { }
    }
}
