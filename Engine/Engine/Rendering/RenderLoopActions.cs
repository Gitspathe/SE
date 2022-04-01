using SE.Components;
using SE.Core;
using SE.World;

namespace SE.Rendering
{
    public interface IRenderLoopAction
    {
        string Name { get; }
        void Invoke(Camera2D camera);
    }

    public class LoopCulling : IRenderLoopAction
    {
        public string Name => "Culling";
        public void Invoke(Camera2D camera) => Core.Rendering.PerformCulling(camera);
    }

    public class LoopRenderTargets : IRenderLoopAction
    {
        public string Name => "Draw render targets";
        public void Invoke(Camera2D camera) => Core.Rendering.DrawRenderTargets(camera);
    }

    public class LoopFinalizeParticles : IRenderLoopAction
    {
        public string Name => "Finalize asyncronous particles update task";
        public void Invoke(Camera2D camera) => ParticleEngine.WaitForThreads();
    }

    public class LoopConsoleUnderlay : IRenderLoopAction
    {
        public string Name => "Draw console overlay";
        public void Invoke(Camera2D camera) => Console.DrawUnderlay(camera, Core.Rendering.SpriteBatch);
    }

    public class LoopLighting : IRenderLoopAction
    {
        public string Name => "Update lighting";
        public void Invoke(Camera2D camera) => Core.Lighting.Update(camera);
    }

    public abstract class DefaultRendererAction : IRenderLoopAction
    {
        public abstract string Name { get; }
        public Renderer Renderer { get; }
        public DefaultRendererAction(Renderer renderer) => Renderer = renderer;
        public abstract void Invoke(Camera2D camera);
    }

    public class LoopPrepareFrame : DefaultRendererAction
    {
        public override string Name => "Prepare frame";
        public override void Invoke(Camera2D camera) => Renderer.NewFrame(camera);
        public LoopPrepareFrame(Renderer renderer) : base(renderer) { }
    }

    public class LoopGenerateRenderLists : DefaultRendererAction
    {
        public override string Name => "Generate render lists";
        public override void Invoke(Camera2D camera) => Renderer.GenerateRenderLists();
        public LoopGenerateRenderLists(Renderer renderer) : base(renderer) { }
    }

    public class LoopProcessRenderList : DefaultRendererAction
    {
        public override string Name => "Process render list: " + renderList.Mode;
        private RenderList renderList;

        public override void Invoke(Camera2D camera) => Renderer.ProcessRenderList(camera, renderList);
        public LoopProcessRenderList(Renderer renderer, RenderList renderList) : base(renderer) => this.renderList = renderList;
    }

    public class LoopProcessUnorderedRenderList : DefaultRendererAction
    {
        public override string Name => "Process unordered render list: " + renderList;
        private UnorderedRenderList renderList;

        public override void Invoke(Camera2D camera) => Renderer.ProcessUnorderedRenderList(camera, renderList);
        public LoopProcessUnorderedRenderList(Renderer renderer, UnorderedRenderList renderList) : base(renderer) => this.renderList = renderList;
    }

    public class LoopStartLighting : DefaultRendererAction
    {
        public override string Name => "Start lighting";
        public override void Invoke(Camera2D camera) => Renderer.StartLighting();
        public LoopStartLighting(Renderer renderer) : base(renderer) { }
    }

    public class LoopEndLighting : DefaultRendererAction
    {
        public override string Name => "End lighting";
        public override void Invoke(Camera2D camera) => Renderer.EndLighting();
        public LoopEndLighting(Renderer renderer) : base(renderer) { }
    }

    public class LoopParticles : DefaultRendererAction
    {
        public override string Name => "Draw particles";
        public override void Invoke(Camera2D camera) => Renderer.DrawParticles(camera);
        public LoopParticles(Renderer renderer) : base(renderer) { }
    }

    public class LoopUI : DefaultRendererAction
    {
        public override string Name => "DrawUI";
        public override void Invoke(Camera2D camera) => Renderer.DrawUI(camera);
        public LoopUI(Renderer renderer) : base(renderer) { }
    }

    public class NewRender : DefaultRendererAction
    {
        public override string Name => "New Render";
        public override void Invoke(Camera2D camera) => NeoRenderer.RenderingManager.Render(camera);
        public NewRender(Renderer renderer) : base(renderer) { }
    }

    // TODO: This probably shouldn't be a single render action. (Material could handle render order)
    public class LoopTileMaps : DefaultRendererAction
    {
        public override string Name => "Draw Tile Maps";
        public override void Invoke(Camera2D camera) => TileMapRendererManager.Render(camera);
        public LoopTileMaps(Renderer renderer) : base(renderer) { }
    }
}
