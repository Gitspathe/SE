using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SE.Utility;

namespace SE.Rendering
{
    public static class DrawCallDatabase
    {
        public static QuickList<DrawCall> LookupArray = new QuickList<DrawCall>();
        private static Dictionary<DrawCall, int> drawCallDictionary = new Dictionary<DrawCall, int>(128, new DrawCallComparer());

        private static int curIndex;

        public static int TryGetID(DrawCall drawCall)
        {
            if (drawCall.Texture == null)
                throw new InvalidOperationException("The DrawCall does not have a texture!");
            if (drawCallDictionary.TryGetValue(drawCall, out int i))
                return i;

            // Create entry
            drawCallDictionary.Add(drawCall, curIndex);
            LookupArray.Add(drawCall);
            curIndex++;

            // TODO: If running out of space, find empty spaces to insert entries.

            return curIndex - 1;
        }

        internal static void PruneAsset<T>(T asset)
        {
            switch (asset) {
                case Texture2D texture: {
                    for (int i = 0; i < LookupArray.Count; i++) {
                        Texture2D existingTexture = LookupArray.Array[i].Texture;
                        if(existingTexture == null)
                            return;

                        // Compare name and ContentManager to check for equality.
                        if (existingTexture.Name == texture.Name && existingTexture.ContentManager == texture.ContentManager) {
                            drawCallDictionary.Remove(LookupArray.Array[i]);
                            LookupArray.Array[i] = default;
                        }
                    }
                    break;
                }
                case Effect effect: {
                    for (int i = 0; i < LookupArray.Count; i++) {
                        Effect existingEffect = LookupArray.Array[i].Effect;
                        if(existingEffect == null)
                            return;

                        // Compare name and ContentManager to check for equality.
                        if (existingEffect.Name == effect.Name && existingEffect.ContentManager == effect.ContentManager) {
                            drawCallDictionary.Remove(LookupArray.Array[i]);
                            LookupArray.Array[i] = default;
                        }
                    }
                    break;
                }
            }
        }

    }

}
