namespace SE.AssetManagement.Processors
{
    public interface IAssetProcessor<out TOutput> : IAssetProcessor
    {
        new TOutput Construct();
    }

    public interface IAssetProcessor
    {
        object Construct();
    }
}
