using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace SE.NeoRenderer
{
    public sealed class SpriteEffect : Effect
    {
        private EffectParameter matrixParam;
        private EffectParameter textureParam;
        private Viewport lastViewport;
        private Matrix projection;

        public Matrix? TransformMatrix { get; set; }
        public Texture2D MainTexture { get; set; }

        public SpriteEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            CacheEffectParameters();
        }

        public SpriteEffect(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(
            graphicsDevice, effectCode, index, count)
        {
            CacheEffectParameters();
        }

        private SpriteEffect(Effect cloneSource) : base(cloneSource)
        {
            CacheEffectParameters();
        }

        private void CacheEffectParameters()
        {
            matrixParam = Parameters["MatrixTransform"];
            textureParam = Parameters["MainTexture"];
        }

        protected override void OnApply()
        {
            var vp = GraphicsDevice.Viewport;
            if ((vp.Width != lastViewport.Width) || (vp.Height != lastViewport.Height)) {
                // Normal 3D cameras look into the -z direction (z = 1 is in front of z = 0). The
                // sprite batch layer depth is the opposite (z = 0 is in front of z = 1).
                // --> We get the correct matrix with near plane 0 and far plane -1.
                Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -1, out projection);

                if (GraphicsDevice.UseHalfPixelOffset) {
                    projection.M41 += -0.5f * projection.M11;
                    projection.M42 += -0.5f * projection.M22;
                }

                lastViewport = vp;
            }

            if (TransformMatrix.HasValue) {
                matrixParam.SetValue(TransformMatrix.GetValueOrDefault() * projection);
            } else {
                matrixParam.SetValue(projection);
            }

            textureParam.SetValue(MainTexture ?? SpriteBatchManager.NullTexture);
        }
    }
}
