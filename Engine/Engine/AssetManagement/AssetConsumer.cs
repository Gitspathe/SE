using System.Collections.Generic;

namespace SE.AssetManagement
{
    public class AssetConsumer
    {
        internal HashSet<Asset> ReferencedAssets { get; set; }

        internal void ReferenceAssets()
        {
            if(ReferencedAssets == null)
                ReferencedAssets = new HashSet<Asset>();

            HashSet<Asset> tmp = new HashSet<Asset>(ReferencedAssets);
            foreach (Asset asset in tmp) {
                asset.AddReference(this);
            }
        }

        internal void DereferenceAssets()
        {
            if (ReferencedAssets == null)
                ReferencedAssets = new HashSet<Asset>();

            HashSet<Asset> tmp = new HashSet<Asset>(ReferencedAssets);
            foreach (Asset asset in tmp) {
                asset.RemoveReference(this);
            }
        }

        internal void AddReference(Asset reference)
        {
            if (ReferencedAssets == null)
                ReferencedAssets = new HashSet<Asset>();

            ReferencedAssets.Add(reference);
        }

        internal void RemoveReference(Asset reference)
        {
            if (ReferencedAssets == null)
                ReferencedAssets = new HashSet<Asset>();

            ReferencedAssets.Remove(reference);
        }
    }

    public interface IAssetConsumer
    {
        AssetConsumer AssetConsumer { get; }
    }
}
