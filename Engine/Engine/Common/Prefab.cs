using SE.AssetManagement;
using SE.AssetManagement.Processors;

namespace SE.Common
{

    // TODO: Prefabs are basically GameObjects, which have their components and/or children initialized by either the engine, through serialization,
    // TODO: or by the user, through manual code. GameObject should eventually be made sealed, and prefabs should take the role of specialized GameObjects.

    // TODO: Probably split into Prefab (engine serialized) and UserPrefab (manual). This is to support both an editor, and IDE-only workflow.

    // TODO: Move [Components(...)] attribute here.

    // TODO: Nested prefabs??

    public class Prefab : Asset<Prefab>
    {
        internal Prefab(string id, AssetProcessor processor, ContentLoader contentLoader) : base(id, processor, contentLoader) { }
    }
}
