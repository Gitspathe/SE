using SE.Utility;
using System.Runtime.CompilerServices;

namespace SE.Rendering
{
    public class RenderList
    {
        public QuickList<QuickList<IRenderable>> Data;

        public BlendMode Mode;
        private int newListCapacity;

        /// <summary>
        /// Adds render data to the cache.
        /// </summary>
        /// <param name="key">Draw-call entry.</param>
        /// <param name="rd">RenderData.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int key, IRenderable rd)
        {
            Data.Array[key].Add(rd);
        }

        /// <summary>
        /// Clears current rendering data from the draw-call entries.
        /// </summary>
        public void Reset()
        {
            if (DrawCallDatabase.LookupArray.Count + 1 > Data.Count) {
                Data.Clear();

                int goal = DrawCallDatabase.LookupArray.Count + 1;
                Data = new QuickList<QuickList<IRenderable>>();
                for (int i = 0; i < goal; i++) {
                    Data.Add(new QuickList<IRenderable>(newListCapacity));
                }
            } else {
                foreach (QuickList<IRenderable> list in Data) {
                    list.Clear();
                }
            }
        }

        public RenderList(int renderIndex, BlendMode mode, int capacity = 1, int newListCapacity = 128)
        {
            Data = new QuickList<QuickList<IRenderable>>();
            Mode = mode;
            this.newListCapacity = newListCapacity;
            Reset();
        }
    }
}
