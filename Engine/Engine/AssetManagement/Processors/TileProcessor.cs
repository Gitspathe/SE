using System;
using System.Collections.Generic;
using System.Numerics;
using SE.Common;

namespace SE.AssetManagement.Processors
{
    public class TileProcessor : AssetProcessor
    {
        private Func<Vector2, GameObject> func;

        public override HashSet<IAsset> GetReferencedAssets()
        {
            return null;
        }

        public override object Construct()
        {
            return func;
        }

        public TileProcessor(Func<Vector2, GameObject> func)
        {
            this.func = func;
        }

        public TileProcessor() { }
    }
}
