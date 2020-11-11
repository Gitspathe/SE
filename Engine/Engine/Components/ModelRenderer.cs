using Microsoft.Xna.Framework.Graphics;
using SE.Common;
using SE.Rendering;

namespace SE.Components
{
    public class ModelComponent : Component
    {
        public ModelRenderer Renderer;

        public ModelComponent(Model model, MaterialEffect material)
        {
            Renderer = new ModelRenderer(model, Owner.Transform, material);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Renderer.Enabled = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Renderer.Enabled = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Renderer.Enabled = false;
        }
    }

    public class ModelRenderer
    {
        public bool Enabled;
        public ModelDefinition ModelDefinition;
        public MaterialEffect Material;

        public ModelRenderer(Model model, Transform transform, MaterialEffect material, MeshMaterialLibrary library = null)
        {
            ModelDefinition = new ModelDefinition(model, transform);

            if (library != null)
                RegisterInLibrary(library);
        }

        public void RegisterInLibrary(MeshMaterialLibrary library)
        {
            library.Register(Material, ModelDefinition.Model, ModelDefinition.Transform);
        }
    }
}
