﻿using Microsoft.Xna.Framework;
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

        private static SpriteBatcher[] spriteBatchers;
        private static SpriteBatchManagerObserver observer = new SpriteBatchManagerObserver();

        private static bool initialized;

        internal static void Initialize()
        {
            if(initialized)
                return;

            spriteBatchers = new SpriteBatcher[RenderConfig.MaxMaterialSlots];

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

            if (spriteBatchers[mat.MaterialID] == null) {
                SpriteBatcher batcher = new SpriteBatcher();
                batcher.Configure(mat);
                batcher.Initialize();
                spriteBatchers[mat.MaterialID] = batcher;
            }
        }

        internal static void ConfigureNextDraw(Matrix? matrix)
        {
            TransformMatrix = matrix;
            DefaultEffect.TransformMatrix = TransformMatrix;
        }

        internal static void EnsureNextFrameCapacity()
        {
            for (int i = 0; i < spriteBatchers.Length; i++) {
                spriteBatchers[i]?.EnsureNextFrameCapacity();
            }
        }

        internal static SpriteBatcher GetBatcher(int materialID)
        {
            return spriteBatchers[materialID];
        }

        // TODO: Pooling and shiet.
        internal static void RemoveBatcher(SpriteMaterial material)
        {
            SpriteBatcher batcher = spriteBatchers[material.MaterialID];
            if (batcher != null) {
                batcher.Clear();
                spriteBatchers[material.MaterialID] = null;
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
