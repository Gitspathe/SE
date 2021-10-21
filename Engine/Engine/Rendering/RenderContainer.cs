using SE.Utility;
using System.Collections.Generic;

namespace SE.Rendering
{
    public sealed class RenderContainer
    {
        // Weird, but is faster than SortedDictionary. Might get more benefit from adding
        // an additional QuickList which stores the indexes of ACTUAL RenderList elements. (??)
        // Note: ^^^ WTF is this supposed to mean?
        public QuickList<RenderList> RenderLists = new QuickList<RenderList>();
        public QuickList<UnorderedRenderList> UnorderedRenderLists = new QuickList<UnorderedRenderList>();

        private Dictionary<uint, IRenderLoopAction> renderActions = new Dictionary<uint, IRenderLoopAction>();
        private Dictionary<uint, IRenderLoopAction> unorderedRenderActions = new Dictionary<uint, IRenderLoopAction>();
        private bool isDirty;

        public void Clear()
        {
            RenderLists.Clear();
        }

        public void Reset()
        {
            RenderList[] lists = RenderLists.Array;
            for (int i = 0; i < RenderLists.Count; i++) {
                lists[i]?.Reset();
            }

            UnorderedRenderList[] unorderedLists = UnorderedRenderLists.Array;
            for (int i = 0; i < UnorderedRenderLists.Count; i++) {
                unorderedLists[i]?.Reset();
            }
        }

        public void Add(IRenderable renderObj)
        {
            Material material = renderObj.Material;

            // Index of the specific RenderList the sprite should be added to.
            int renderIndex = (int)material.RenderQueueInternal;

            // Add the sprite to the correct RenderList.
            if (!material.RequiresUnorderedInternal) {
                while (RenderLists.Count < renderIndex + 1) {
                    RenderLists.Add(null);
                }

                RenderList list = RenderLists.Array[renderIndex];
                if (list == null) {
                    list = new RenderList(renderIndex, material.BlendMode);
                    RenderLists.Array[renderIndex] = list;
                    isDirty = true;
                }
                list.Add(material.DrawCallID, renderObj);
            } else {
                while (UnorderedRenderLists.Count < renderIndex + 1) {
                    UnorderedRenderLists.Add(null);
                }

                UnorderedRenderList list = UnorderedRenderLists.Array[renderIndex];
                if (list == null) {
                    list = new UnorderedRenderList();
                    UnorderedRenderLists.Array[renderIndex] = list;
                    isDirty = true;
                }
                list.Add(renderObj);
            }
        }

        public void RegisterToRenderLoop()
        {
            if (!isDirty)
                return;

            Renderer renderer = RenderLoop.Render;

            // Clear ordered render list actions.
            foreach ((uint key, _) in renderActions) {
                RenderLoop.Loop.Remove(key);
            }
            renderActions.Clear();

            // Clear unordered render list actions.
            foreach ((uint key, _) in unorderedRenderActions) {
                RenderLoop.Loop.Remove(key);
            }
            unorderedRenderActions.Clear();

            // Ensure transparent objects are always rendered after opaque.
            // FIRST - Add pre-ordered render actions to the rendering loop.
            for (uint i = 0; i < RenderLists.Count; i++) {
                RenderList list = RenderLists.Array[i];
                if (list == null)
                    continue;

                IRenderLoopAction action = new LoopProcessRenderList(renderer, list);
                renderActions.Add(i, action);
                RenderLoop.Add(i, action);
            }
            // NEXT - Add unordered render actions to the rendering loop (for proper transparency).
            for (uint i = 0; i < UnorderedRenderLists.Count; i++) {
                UnorderedRenderList list = UnorderedRenderLists.Array[i];
                if (list == null)
                    continue;

                IRenderLoopAction action = new LoopProcessUnorderedRenderList(renderer, list);
                unorderedRenderActions.Add(i, action);
                RenderLoop.Add(i, action);
            }

            isDirty = false;
        }
    }
}
