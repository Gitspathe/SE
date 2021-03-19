using System.Runtime.CompilerServices;
using SE.Utility;

namespace SE.Rendering
{
    public class UnorderedRenderList
    {
        public QuickList<IRenderable> Data;

        /// <summary>
        /// Adds render data to the cache.
        /// </summary>
        /// <param name="key">Draw-call entry.</param>
        /// <param name="rd">RenderData.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(IRenderable rd)
        {
            Data.Add(rd);
        }

        /// <summary>
        /// Clears current rendering data from the draw-call entries.
        /// </summary>
        public void Reset()
        {
            Data.Clear();
        }

        public UnorderedRenderList()
        {
            Data = new QuickList<IRenderable>();
            Reset();
        }
    }
}
