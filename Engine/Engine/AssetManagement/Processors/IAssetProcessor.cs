using System.Collections.Generic;

namespace SE.AssetManagement.Processors
{
    public abstract class AssetProcessor : IAssetProcessor
    {
        public abstract HashSet<IAsset> GetReferencedAssets();
        public abstract object Construct();
    }

    public interface IAssetProcessor
    {
        object Construct();
    }
}
