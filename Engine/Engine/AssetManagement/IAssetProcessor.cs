namespace SE.AssetManagement
{
    public interface IAssetProcessor<out TOutput> : IAssetProcessor
    {
        public TOutput Construct();
    }

    public interface IAssetProcessor
    {
        object Construct();
    }
}
