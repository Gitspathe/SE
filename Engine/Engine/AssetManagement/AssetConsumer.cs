using System.Collections.Generic;

namespace SE.AssetManagement
{
    public class AssetConsumer
    {
        public HashSet<IAsset> ReferencedAssets { get; set; }

        public void ReferenceAssets()
        {
            if(ReferencedAssets == null)
                ReferencedAssets = new HashSet<IAsset>();

            HashSet<IAsset> tmp = new HashSet<IAsset>(ReferencedAssets);
            foreach (IAsset asset in tmp) {
                asset.AddReference(this);
            }
        }

        public void DereferenceAssets()
        {
            if (ReferencedAssets == null)
                ReferencedAssets = new HashSet<IAsset>();

            HashSet<IAsset> tmp = new HashSet<IAsset>(ReferencedAssets);
            foreach (IAsset asset in tmp) {
                asset.RemoveReference(this);
            }
        }

        public void AddReference(IAsset reference)
        {
            if (ReferencedAssets == null)
                ReferencedAssets = new HashSet<IAsset>();

            ReferencedAssets.Add(reference);
        }

        public void RemoveReference(IAsset reference)
        {
            if (ReferencedAssets == null)
                ReferencedAssets = new HashSet<IAsset>();

            ReferencedAssets.Remove(reference);
        }
    }

    public interface IAssetConsumer
    {
        public AssetConsumer AssetConsumer { get; }
    }
}
