using SE.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace SE.NeoRenderer
{
    public interface IRenderAction
    {
        uint RenderQueue { get; }

        void Execute();
    }

    internal sealed class ConfigureSpriteBatchManager : IRenderAction
    {
        private Camera2D camera;

        public uint RenderQueue => 0;

        public void Execute()
        {
            SpriteBatchManager.ConfigureNextDraw(camera.ViewMatrix);
        }

        public ConfigureSpriteBatchManager(Camera2D camera)
        {
            this.camera = camera;
        }
    }

    internal sealed class RenderBatchTest : IRenderAction
    {
        public RenderBatchTest(uint queue, SpriteBatcher batcher)
        {
            RenderQueue = queue;
            b = batcher;
        }

        private SpriteBatcher b;

        public uint RenderQueue { get; set; }

        public void Execute()
        {
            b.DrawBatch();
        }
    }

    // These should set the majority of the GraphicsDevice states!

    // SpriteRenderAction, UnorderedSpriteRenderAction...
}
