using SE.Components;
using SE.Components.UI;
using System;
using System.Collections.Generic;

namespace SE.Rendering
{
    public static class RenderableTypeLookup
    {
        private static Dictionary<Type, RenderableTypeInfo> lookup = new Dictionary<Type, RenderableTypeInfo>();

        public static RenderableTypeInfo Retrieve(IRenderable renderable)
        {
            Type t = renderable.GetType();
            if (lookup.TryGetValue(t, out RenderableTypeInfo info)) {
                return info;
            }

            info = new RenderableTypeInfo(renderable);
            lookup.Add(t, info);
            return info;
        }
    }

    public sealed class RenderableTypeInfo
    {
        internal ILit Lit;
        internal IUISprite UISprite;

        internal RenderableTypeInfo(IRenderable renderable)
        {
            Lit = renderable as ILit;
            UISprite = renderable as IUISprite;
        }
    }
}
