using System;
using System.Collections.Generic;

namespace SE.AssetManagement
{
    public class AssetConsumerContext : IAssetConsumer, IDisposable
    { 
        public AssetConsumer AssetConsumer { get; } = new AssetConsumer();

        public void Dispose()
        {
            AssetConsumer?.DereferenceAssets();
        }
    }
}
