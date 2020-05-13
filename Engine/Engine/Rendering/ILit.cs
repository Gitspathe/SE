using SE.Lighting;

namespace SE.Rendering
{
    public interface ILit
    {
        public bool IgnoreLight { get; set; }

        public ShadowCasterType ShadowType { get; set; }

        public ShadowCaster Shadow { get; set; }
    }
}
