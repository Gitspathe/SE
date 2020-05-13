using System.Collections.Generic;

namespace SE.AssetManagement
{
    public class AssetConsumerContext : IAssetConsumer
    {
       HashSet<IAsset> IAssetConsumer.ReferencedAssets { get; set; }

       public void ClearReferences()
       {
           ((IAssetConsumer) this).DereferenceAssets();
       }
    }
}
