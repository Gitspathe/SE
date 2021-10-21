using SE.Components;

namespace SEDemos
{
    public class SpriteSerializer : SpriteBaseSerializer<Sprite>
    {
        protected override void Setup()
        {
            base.Setup();
        }

        public SpriteSerializer(Sprite obj) : base(obj) { }
    }
}
