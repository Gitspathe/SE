using System.Collections.Generic;

namespace SE.AssetManagement
{
    public interface IAssetConsumer
    {
        internal HashSet<IAsset> ReferencedAssets { get; set; }

        internal void ReferenceAssets()
        {
            if(ReferencedAssets == null)
                ReferencedAssets = new HashSet<IAsset>();

            HashSet<IAsset> tmp = new HashSet<IAsset>(ReferencedAssets);
            foreach (IAsset asset in tmp) {
                asset.AddReference(this);
            }
        }

        internal void DereferenceAssets()
        {
            if (ReferencedAssets == null)
                ReferencedAssets = new HashSet<IAsset>();

            HashSet<IAsset> tmp = new HashSet<IAsset>(ReferencedAssets);
            foreach (IAsset asset in tmp) {
                asset.RemoveReference(this);
            }
        }

        internal void AddReference(IAsset reference)
        {
            if (ReferencedAssets == null)
                ReferencedAssets = new HashSet<IAsset>();

            ReferencedAssets.Add(reference);
        }

        internal void RemoveReference(IAsset reference)
        {
            if (ReferencedAssets == null)
                ReferencedAssets = new HashSet<IAsset>();

            ReferencedAssets.Remove(reference);
        }
    }
}
