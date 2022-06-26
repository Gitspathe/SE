using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SE.NeoRenderer
{
    internal static class SpriteBatchManager
    {
        internal static SpriteEffect DefaultEffect;
        internal static EffectPass DefaultEffectPass;
        internal static Texture2D NullTexture;
        internal static Matrix? TransformMatrix;

        private static Dictionary<uint, SpriteBatcher> spriteBatchers = new Dictionary<uint, SpriteBatcher>();
        private static SpriteBatchManagerObserver observer = new SpriteBatchManagerObserver();

        private static bool initialized;

        internal static void Initialize()
        {
            if(initialized)
                return;

            SpriteMaterialHandler.RegisterMaterialObserver(observer);
            initialized = true;

            DefaultEffect = ShaderLoader.LoadSpriteEffect("Sprite");
            DefaultEffectPass = DefaultEffect.Techniques[0].Passes[0];

            Color[] nullData = new Color[64 * 64];
            for (int i = 0; i < 64 * 64; i++) {
                nullData[i] = Color.Magenta;
            }
            NullTexture = new Texture2D(GameEngine.Engine.GraphicsDevice, 64, 64);
            NullTexture.SetData(nullData);
        }

        internal static void CreateNewSpriteBatchIfNeeded(SpriteMaterial mat)
        {
            // Use a more fucked up system to handle transparent objects.
            if (mat.SpecialRenderOrdering) {
                return; // I will need to add it to an "UnorderedSpriteBatcher" instead.
            }

            if (!spriteBatchers.ContainsKey(mat.MaterialID)) {
                SpriteBatcher batcher = new SpriteBatcher();
                batcher.Configure(mat);
                batcher.Initialize();
                spriteBatchers.Add(mat.MaterialID, batcher);
            }
        }

        internal static void ConfigureNextDraw(Matrix? matrix)
        {
            TransformMatrix = matrix;
            DefaultEffect.TransformMatrix = TransformMatrix;
        }

        internal static void EnsureNextFrameCapacity()
        {
            foreach (SpriteBatcher spriteBatcher in spriteBatchers.Values) {
                spriteBatcher.EnsureNextFrameCapacity();
            }
        }

        internal static SpriteBatcher GetBatcher(SpriteMaterial material)
        {
            return spriteBatchers[material.MaterialID];
        }

        internal static SpriteBatcher GetBatcher(uint materialID)
        {
            return spriteBatchers[materialID];
        }

        // TODO: Pooling and shiet.
        internal static void RemoveBatcher(SpriteMaterial material)
        {
            if(spriteBatchers.TryGetValue(material.MaterialID, out SpriteBatcher val)) {
                val.Clear();
                spriteBatchers.Remove(material.MaterialID);
            }
        }

        private class SpriteBatchManagerObserver : IMaterialObserver
        {
            public void MaterialCreated(SpriteMaterial material)
            {
                CreateNewSpriteBatchIfNeeded(material);
            }

            public void MaterialPropertyChanged(SpriteMaterial material)
            {
                // Remove and then add again.
                RemoveBatcher(material);
                CreateNewSpriteBatchIfNeeded(material);
            }

            public void MaterialDispose(SpriteMaterial material)
            {
                RemoveBatcher(material);
            }
        }
    }
}
