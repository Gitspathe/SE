using System;
using System.Collections.Generic;
using SE.Components;

namespace SE.Rendering
{
    public class RenderContainer
    {
        public SortedDictionary<int, RenderList> RenderLists = new SortedDictionary<int, RenderList>();

        private bool isDirty;

        public void Clear()
        {
            RenderLists.Clear();
        }

        public void Reset()
        {
            foreach (KeyValuePair<int, RenderList> pair in RenderLists) {
                pair.Value.Reset();
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

            // Add the sprite to the correct RenderList.
            if (RenderLists.TryGetValue(renderIndex, out RenderList list)) {
                if (threadSafe) {
                    list.AddThreaded(sprite.DrawCallID, sprite);
                } else {
                    list.Add(sprite.DrawCallID, sprite);
                }
            } else {
                // If the RenderList does not exist, create it.
                if (threadSafe) {
                    lock (RenderLists) {
                        if (RenderLists.ContainsKey(renderIndex))
                            return;

                        RenderList newList = new RenderList(renderIndex, sprite.BlendMode); 
                        newList.AddThreaded(sprite.DrawCallID, sprite);
                        RenderLists.Add(renderIndex, newList);
                    }
                } else {
                    RenderList newList = new RenderList(renderIndex, sprite.BlendMode);
                    newList.Add(sprite.DrawCallID, sprite);
                    RenderLists.Add(renderIndex, newList);
                }
                isDirty = true;
            }
        }

        public void RegisterToRenderLoop()
        {
            if(!isDirty)
                return;

            DefaultRenderer renderer = RenderLoop.DefaultRender;
            foreach ((int loopOrder, RenderList list) in RenderLists) {
                if (!RenderLoop.Loop.ContainsKey(loopOrder)) {
                    RenderLoop.Add(loopOrder, cam => renderer.ProcessRenderList(cam, list));
                }
            }
            isDirty = false;
        }
    }
}
