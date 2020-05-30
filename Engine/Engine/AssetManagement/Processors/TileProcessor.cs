using System;
using System.Numerics;
using SE.Common;

namespace SE.AssetManagement.Processors
{
    public class TileProcessor : IAssetProcessor<Func<Vector2, GameObject>>
    {
        private Func<Vector2, GameObject> func;

        public Func<Vector2, GameObject> Construct()
        {
            return func;
        }

        object IAssetProcessor.Construct()
        {
            return Construct();
        }

        public TileProcessor(Func<Vector2, GameObject> func)
        {
            this.func = func;
        }

        public TileProcessor() { }
    }
}
