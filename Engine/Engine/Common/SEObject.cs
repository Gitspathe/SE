using SE.AssetManagement;
using SE.Serialization;

namespace SE.Common
{
    public class SEObject : ISerializedObject, IAssetConsumer
    {
        public ulong InstanceID { get; internal set; }

        public AssetConsumer AssetConsumer => AssetConsumerInternal ??= new AssetConsumer();
        internal AssetConsumer AssetConsumerInternal;

        public virtual void Destroy()
        {
            AssetConsumerInternal?.DereferenceAssets();
        }
    }
}
