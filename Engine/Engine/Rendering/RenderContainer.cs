using System;
using System.Collections.Generic;
using SE.Components;
using SE.Utility;

namespace SE.Rendering
{
    public class RenderContainer
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

        public void Add(IRenderable renderObj, RenderableData info)
        {
            Material material = info.Material;

            // Determine if the sprite ignores light or not.
            bool ignoreLight = true;
            if (info.Lit != null) {
                ignoreLight = info.Lit.IgnoreLight;
            }

            // Index of the specific RenderList the sprite should be added to.
            int renderIndex = (int) (ignoreLight ? RenderLoop.LoopEnum.AfterLighting : RenderLoop.LoopEnum.DuringLighting);
            bool requiresUnordered = false;

            // Determine if the sprite is transparent.
            BlendMode blendMode = material.BlendMode;
            switch (blendMode) {
                case BlendMode.Opaque:
                    renderIndex += 100;
                    break;
                case BlendMode.Transparent:
                    renderIndex += 1000;
                    requiresUnordered = true;
                    break;
                case BlendMode.Additive:
                    renderIndex += 2000;
                    requiresUnordered = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Add the sprite to the correct RenderList.
            if (requiresUnordered) {
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
            } else {
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
            }
        }

        public void RegisterToRenderLoop()
        {
            if(!isDirty)
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
