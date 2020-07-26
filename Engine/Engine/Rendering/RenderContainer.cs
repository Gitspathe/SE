using System;
using System.Collections.Generic;
using SE.Components;
using SE.Utility;

namespace SE.Rendering
{
    public class RenderContainer
    {
        // Weird, but is faster than SortedDictionary. Might get more benefit from adding
        // an additional QuickList which stores the indexes of ACTUAL RenderList elements.
        public QuickList<RenderList> RenderLists = new QuickList<RenderList>();
        
        private Dictionary<uint, IRenderLoopAction> renderActions = new Dictionary<uint, IRenderLoopAction>();
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
        }

        public void Add(IRenderable renderObj, RenderableTypeInfo typeInfo, bool threadSafe = false)
        {
            // Determine if the sprite ignores light or not.
            bool ignoreLight = true;
            if (typeInfo.lit != null) {
                ignoreLight = typeInfo.lit.IgnoreLight;
            }

            // Index of the specific RenderList the sprite should be added to.
            int renderIndex = (int) (ignoreLight ? RenderLoop.LoopEnum.AfterLighting : RenderLoop.LoopEnum.DuringLighting);

            // Determine if the sprite is transparent.
            BlendMode blendMode = renderObj.BlendMode;
            switch (blendMode) {
                case BlendMode.Opaque:
                    renderIndex += 100;
                    break;
                case BlendMode.Transparent:
                    renderIndex += 1000;
                    break;
                case BlendMode.Additive:
                    renderIndex += 2000;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            while (RenderLists.Count < renderIndex + 1) {
                RenderLists.Add(null);
            }

            // Add the sprite to the correct RenderList.
            RenderList list = RenderLists.Array[renderIndex];
            if (list != null) {
                if (threadSafe) {
                    list.AddThreaded(renderObj.DrawCallID, renderObj);
                } else {
                    list.Add(renderObj.DrawCallID, renderObj);
                }
            } else {
                if (threadSafe) {
                    lock (RenderLists) {
                        //if (RenderLists.ContainsKey(renderIndex))
                        //    return;

                        RenderList newList = new RenderList(renderIndex, renderObj.BlendMode);
                        newList.AddThreaded(renderObj.DrawCallID, renderObj);
                        RenderLists.Array[renderIndex] = newList;
                    }
                } else {
                    RenderList newList = new RenderList(renderIndex, renderObj.BlendMode);
                    newList.Add(renderObj.DrawCallID, renderObj);
                    RenderLists.Array[renderIndex] = newList;
                }
                isDirty = true;
            }
        }

        public void RegisterToRenderLoop()
        {
            if(!isDirty)
                return;

            foreach ((uint key, _) in renderActions) {
                RenderLoop.Loop.Remove(key);
            }
            renderActions.Clear();

            Renderer renderer = RenderLoop.Render;
            for (uint i = 0; i < RenderLists.Count; i++) {
                RenderList list = RenderLists.Array[i];
                if (list == null || RenderLoop.Loop.ContainsKey(i))
                    continue;

                IRenderLoopAction action = new LoopProcessRenderList(renderer, list);
                renderActions.Add(i, action);
                RenderLoop.Add(i, action);
            }
            isDirty = false;
        }
    }
}
