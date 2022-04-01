using SE.Lighting;

namespace SE.Rendering
{

    // TODO: Get rid of this somehow, it's a dumb dependency.
    public interface ILit
    {
        ShadowCasterType ShadowType { get; set; }

        ShadowCaster Shadow { get; set; }
    }
}
