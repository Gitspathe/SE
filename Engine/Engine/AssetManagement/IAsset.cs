using System.Collections.Generic;

namespace SE.AssetManagement
{
    public interface IAsset
    {
        public uint LoadOrder { get; internal set; }
        public bool Loaded { get; internal set; }
        internal HashSet<IAssetConsumer> References { get; set; }

        internal void RemoveReference(IAssetConsumer reference);
        internal void AddReference(IAssetConsumer reference);
        internal void Load();
        internal void Unload();
        internal void Purge();
    }
}
