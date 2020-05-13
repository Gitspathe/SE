using System.Collections.Generic;
using DeeZ.Engine.Utility;
using Microsoft.Xna.Framework.Graphics;

namespace SE.Rendering
{
    public static class DrawCallDatabase
    {
        public static QuickList<DrawCall> LookupArray = new QuickList<DrawCall>();
        private static Dictionary<DrawCall, int> drawCallDictionary = new Dictionary<DrawCall, int>(128, new DrawCallComparer());

        public static int TryGetID(DrawCall drawCall)
        {
            if (drawCall.Texture == null)
                return -1;
            if (drawCallDictionary.TryGetValue(drawCall, out int i))
                return i;

            // Create entry
            drawCallDictionary.Add(drawCall, LookupArray.Count);
            LookupArray.Add(drawCall);

            return LookupArray.Count-1;
        }

        internal static void PruneAsset<T>(T asset)
        {
            switch (asset) {
                case Texture2D texture: {
                    for (int i = 0; i < LookupArray.Count; i++) {
                        Texture2D existingTexture = LookupArray.Array[i].Texture;

                        // Compare name and ContentManager to check for equality.
                        if (existingTexture.Name == texture.Name && existingTexture.ContentManager == texture.ContentManager) {
                            drawCallDictionary.Remove(LookupArray.Array[i]);
                            LookupArray.RemoveAt(i);
                        }
                    }
                    break;
                }
                case Effect effect: {
                    for (int i = 0; i < LookupArray.Count; i++) {
                        Effect existingEffect = LookupArray.Array[i].Effect;

                        // Compare name and ContentManager to check for equality.
                        if (existingEffect.Name == effect.Name && existingEffect.ContentManager == effect.ContentManager) {
                            drawCallDictionary.Remove(LookupArray.Array[i]);
                            LookupArray.RemoveAt(i);
                        }
                    }
                    break;
                }
            }

            // TODO: Regenerate lookup dictionary.
        }

    }

}
