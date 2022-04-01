using Microsoft.Xna.Framework;
using SE.Components;
using SE.Core;
using System;
using System.Collections.Generic;
using System.Text;
using SE.Utility;

namespace SE.NeoRenderer
{
    public static class RenderingManager
    {
        public static RenderPipeline Renderer;

        private static bool initialized;

        public static void Initialize(RenderPipeline renderer)
        {
            if(initialized)
                throw new Exception();

            // temporary, move to default pipeline when done!
            SpriteBatchManager.Initialize();

            Renderer = renderer;
            renderer.Initialize();
            initialized = true;
        }

        public static void Render(Camera2D camera)
        {
            if (!initialized)
                throw new Exception();

            Renderer.Run(camera);
        }
    }

    public abstract class RenderPipeline
    {
        private readonly QuickList<IRenderController> controllers = new QuickList<IRenderController>();
        private readonly ControllerComparer comparer = new ControllerComparer();

        protected RenderActionContainer RenderActions { get; } = new RenderActionContainer();

        protected void Register(IRenderController controller)
        {
            controllers.Add(controller);
        }

        /// <summary>
        /// Should instantiate and register IRenderControllers.
        /// </summary>
        public abstract void Setup();

        internal void Initialize()
        {
            Setup();
            controllers.Sort(comparer);
            for(int i = 0; i < controllers.Count; i++) {
                controllers.Array[i].Initialize();
            }
        }

        internal void Run(Camera2D camera)
        {
            RenderActions.Clear();

            // Prepare the render actions.
            for (int i = 0; i < controllers.Count; i++) {
                controllers.Array[i].PrepareActions(RenderActions, camera);
            }

            // Execute the render actions.
            RenderActions.Execute();
        }

        private struct ControllerComparer : IComparer<IRenderController>
        {
            int IComparer<IRenderController>.Compare(IRenderController x, IRenderController y)
                => x.UpdateOrder.CompareTo(y.UpdateOrder);
        }
    }

    public class DefaultRenderPipeline : RenderPipeline
    {
        public override void Setup()
        {
            Register(new SpritesRenderController());
        }
    }
}
