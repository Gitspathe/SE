using System;
using System.Collections.Generic;
using System.Text;

namespace SE.Engine.AssetManagement
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
