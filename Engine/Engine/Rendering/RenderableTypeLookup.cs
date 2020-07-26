using SE.Components.UI;
using System;
using System.Collections.Generic;

namespace SE.Rendering
{
    public static class RenderableTypeLookup
    {
        private static Dictionary<Type, RenderableTypeCache> lookup = new Dictionary<Type, RenderableTypeCache>();

        public static RenderableTypeCache Retrieve(IRenderable renderable)
        {
            Type t = renderable.GetType();
            if (lookup.TryGetValue(t, out RenderableTypeCache info)) {
                return info;
            }

            info = new RenderableTypeCache(renderable);
            lookup.Add(t, info);
            return info;
        }
    }

    public class RenderableTypeCache
    {
        internal ILit lit;
        internal IUISprite uiSprite;

        internal RenderableTypeCache(IRenderable renderable)
        {
            lit = renderable as ILit;
            uiSprite = renderable as IUISprite;
        }
    }
}
