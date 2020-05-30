using System.Collections.Generic;

namespace SE.AssetManagement
{
    public class AssetConsumerContext : IAssetConsumer
    { 
        public AssetConsumer AssetConsumer { get; } = new AssetConsumer();
        
        public void ClearReferences() 
        { 
            AssetConsumer.DereferenceAssets();
        }
    }
}
