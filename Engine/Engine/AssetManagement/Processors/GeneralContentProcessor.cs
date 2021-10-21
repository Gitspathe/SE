using SE.Core;
using System;
using System.Collections.Generic;

namespace SE.AssetManagement.Processors
{
    public class GeneralContentProcessor<T> : AssetProcessor
    {
        private ContentLoader loader;
        private string contentLoaderID;
        private string assetID;

        public override HashSet<Asset> GetReferencedAssets()
        {
            return null;
        }

        public override object Construct()
        {
            ContentLoader loader = AssetManager.GetLoader(contentLoaderID);
            if (loader == null)
                throw new NullReferenceException();

            return loader.Load<T>(assetID);
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
