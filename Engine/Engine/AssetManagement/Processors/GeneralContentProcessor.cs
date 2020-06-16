using System;
using SE.Core;

namespace SE.AssetManagement.Processors
{
    public class GeneralContentProcessor<T> : IAssetProcessor<T>
    {
        private ContentLoader loader;
        private string contentLoaderID;
        private string assetID;

        public T Construct()
        {
            ContentLoader loader = AssetManager.GetLoader(contentLoaderID);
            if (loader == null)
                throw new NullReferenceException();

            return loader.Load<T>(assetID);
        }

        object IAssetProcessor.Construct()
        {
            return Construct();
        }

        public GeneralContentProcessor(string contentLoaderID, string assetID)
        {
            this.contentLoaderID = contentLoaderID;
            this.assetID = assetID;
            loader = AssetManager.GetLoader(contentLoaderID);
            loader.PreloadFiles.Add(assetID);
        }

        public GeneralContentProcessor() { }
    }
}
