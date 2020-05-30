using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using SE.Core;
using SE.Core.Exceptions;

namespace SE.AssetManagement
{
    public class ContentLoader : ContentManager
    {
        public string ID { get; }

        /// <summary>Holds current asset references.</summary>
        internal HashSet<IAsset> References = new HashSet<IAsset>();
        /// <summary>Holds all asset references, including ones which aren't active.</summary>
        internal HashSet<IAsset> AllRefs = new HashSet<IAsset>();

        public bool Inactive => timeInactive >= 10.0f;

        private bool loaded;
        private float timeInactive;
        private List<IAsset> orderedReferences = new List<IAsset>();

        public ContentLoader(IServiceProvider serviceProvider, string id, string rootDirectory) : base(serviceProvider, rootDirectory)
        {
            ID = id;
            AssetManager.AddContentManager(this);
        }

        public override T Load<T>(string name)
        {
            try {
                return base.Load<T>(name);
            } catch (ArgumentNullException e) {
                if (Screen.IsFullHeadless) {
                    throw new HeadlessNotSupportedException("Content could not be loaded in fully headless display mode.", e);
                } 
                throw;
            }
        }

        private T LoadInternal<T>(string name) => base.Load<T>(name);

        public void Update(float deltaTime)
        {
            if(Inactive)
                return;

            if (References.Count < 1)
                timeInactive += deltaTime;
            else
                timeInactive = 0.0f;

            if (Inactive && loaded)
                Unload();
        }

        public void Reload()
        {
            loaded = true;
            timeInactive = 0.0f;

            // Sort all references.
            orderedReferences.Clear();
            orderedReferences.AddRange(AllRefs);
            orderedReferences.Sort((x, y) => x.LoadOrder.CompareTo(y.LoadOrder));

            // Reload all references in order from lowest to highest LoadOrder.
            // All references are loaded initially in order to build up the dependencies.
            foreach (IAsset reference in orderedReferences) {
                reference.Load();
            }

            // Remove unneeded references. TODO: Might cause an exception, investigate.
            foreach (IAsset reference in orderedReferences) {
                reference.Purge();
            }
        }

        internal void AddReference(IAsset reference)
        {
            References.Add(reference);
            AllRefs.Add(reference);
            if (!loaded && References.Count > 0) {
                Reload();
            }
        }

        internal void RemoveReference(IAsset reference)
        {
            References.Remove(reference);

            // Clear stale entries.
            HashSet<IAsset> copy = new HashSet<IAsset>(References);
            foreach (IAsset assetRef in copy) {
                assetRef.Purge();
            }
        }

        public override void Unload()
        {
            loaded = false;

            // Unload all assets from this content loader from memory.
            foreach (IAsset asset in AllRefs) {
                asset.Unload();
            }
            base.Unload();
        }
    }
}
