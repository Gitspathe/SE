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

    public class RenderableTypeInfo
    {
        internal ILit lit;
        internal IUISprite uiSprite;

        internal RenderableTypeInfo(IRenderable renderable)
        {
            lit = renderable as ILit;
            uiSprite = renderable as IUISprite;
        }
    }
}
