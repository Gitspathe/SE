using System;
using System.Collections.Generic;
using SE.Components;
using SE.Engine.Utility;

namespace SE.Rendering
{
    public class RenderContainer
    {
        // Weird, but is faster than SortedDictionary. Might get more benefit from adding
        // an additional QuickList which stores the indexes of ACTUAL RenderList elements.
        public QuickList<RenderList> RenderLists = new QuickList<RenderList>();

        private bool isDirty;

        public void Clear()
        {
            RenderLists.Clear();
        }

        public void Reset()
        {
            for (int i = 0; i < RenderLists.Count; i++) {
                RenderLists.Array[i]?.Reset();
            }
        }

        public void Add(SpriteBase sprite, bool threadSafe = false)
        {
            // Determine if the sprite ignores light or not.
            bool ignoreLight = true;
            if (sprite is ILit lit) {
                ignoreLight = lit.IgnoreLight;
            }

            // Index of the specific RenderList the sprite should be added to.
            int renderIndex = (int) (ignoreLight ? RenderLoop.LoopEnum.AfterLighting : RenderLoop.LoopEnum.DuringLighting);

            // Determine if the sprite is transparent.
            BlendMode blendMode = sprite.blendMode;
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
                    list.AddThreaded(sprite.DrawCallID, sprite);
                } else {
                    list.Add(sprite.DrawCallID, sprite);
                }
            } else {
                if (threadSafe) {
                    lock (RenderLists) {
                        //if (RenderLists.ContainsKey(renderIndex))
                        //    return;

                        RenderList newList = new RenderList(renderIndex, sprite.BlendMode);
                        newList.AddThreaded(sprite.DrawCallID, sprite);
                        RenderLists.Array[renderIndex] = newList;
                    }
                } else {
                    RenderList newList = new RenderList(renderIndex, sprite.BlendMode);
                    newList.Add(sprite.DrawCallID, sprite);
                    RenderLists.Array[renderIndex] = newList;
                }
                isDirty = true;
            }
        }

        public void RegisterToRenderLoop()
        {
            if(!isDirty)
                return;

            DefaultRenderer renderer = RenderLoop.DefaultRender;
            for (int i = 0; i < RenderLists.Count; i++) {
                RenderList list = RenderLists.Array[i];
                if (list == null) 
                    continue;

                if (!RenderLoop.Loop.ContainsKey(i)) {
                    RenderLoop.Add(i, cam => renderer.ProcessRenderList(cam, list));
                }
            }
            isDirty = false;
        }
    }
}
