using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SE.AssetManagement.FileProcessors;
using SE.Core;
using SE.Core.Exceptions;
using SE.Utility;
using Console = SE.Core.Console;

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

        private Dictionary<Type, FileProcessor> fileProcessors = new Dictionary<Type, FileProcessor>();

        private string rootDirectory;
        private GraphicsDevice gfxDevice;
        private bool loaded;
        private float timeInactive;
        private List<IAsset> orderedReferences = new List<IAsset>();

        public ContentLoader(IServiceProvider serviceProvider, string id, string rootDirectory) : base(serviceProvider, rootDirectory)
        {
            ID = id;
            this.rootDirectory = rootDirectory;
            if (!Screen.IsFullHeadless) {
                gfxDevice = ((IGraphicsDeviceService) serviceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            }

            AddProcessor(new Texture2DFileProcessor());
            LocateFiles();
            SetupStreamContent();
            AssetManager.AddContentManager(this);
        }

        public void AddProcessor(FileProcessor processor)
        {
            if(fileProcessors.ContainsKey(processor.Type))
                return;

            processor.ContentBaseDirectory = rootDirectory;
            fileProcessors.Add(processor.Type, processor);
        }

        private void LocateFiles()
        {
            foreach (FileProcessor processor in fileProcessors.Values) {
                processor.ProcessFiles();
                processor.LocateFiles();
            }
        }

        private void SetupStreamContent()
        {
            foreach (FileProcessor processor in fileProcessors.Values) {
                try {
                    processor.LoadFiles(gfxDevice);
                } catch (HeadlessNotSupportedException e) {
                    Console.LogWarning(e.Message);
                }
            }
        }

        private void UnloadStreamContent()
        {
            foreach (FileProcessor processor in fileProcessors.Values) {
                processor.Unload();
            }
        }

        internal static string FormatFilePath(string filePath) 
            => Path.ChangeExtension(filePath, null)?.Replace('\\', '/');

        public override T Load<T>(string name)
        {
            try {

                // Try to return from a FileProcessor.
                if (fileProcessors.TryGetValue(typeof(T), out FileProcessor processor)) {
                    //return (T) processor.LoadSingleFile(gfxDevice, name);
                    if (processor.GetFile(gfxDevice, name, out object file)) {
                        return (T) file;
                    }
                }

                return base.Load<T>(name);
            } catch (ArgumentNullException e) {
                if (Screen.IsFullHeadless)
                    throw new HeadlessNotSupportedException("Content could not be loaded in fully headless display mode.", e);

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
            SetupStreamContent();

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
            UnloadStreamContent();
            base.Unload();
        }
    }
}
