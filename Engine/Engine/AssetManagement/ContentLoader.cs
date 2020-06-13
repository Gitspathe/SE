using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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

        private QuickList<IDisposable> streamDisposables = new QuickList<IDisposable>();
        private static readonly string[] imgExtensions = { ".png", ".bmp", ".jpg", ".gif", ".tif" }; 
        private Dictionary<string, object> streamTextures = new Dictionary<string, object>();
        private HashSet<string> imageFilePaths = new HashSet<string>();

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

            LocateFiles();
            SetupStreamContent();
            AssetManager.AddContentManager(this);
        }

        // TODO: Split stream logic into 'IStreamableProcessor' ??
        private void LocateFiles()
        {
            string baseDir = FileIO.BaseDirectory;
            string path = Path.Combine(baseDir, rootDirectory) + Path.DirectorySeparatorChar;
            string imageDirectory = Path.Combine(path, "Images");

            // Process images.
            if (Directory.Exists(imageDirectory)) {
                foreach (string file in FileIO.GetAllFiles(imageDirectory, imgExtensions)) {
                    imageFilePaths.Add(file);
                }
            }

            // TODO: Others...
        }

        private void SetupStreamContent()
        {
            string baseDir = FileIO.BaseDirectory;
            string path = Path.Combine(baseDir, rootDirectory) + Path.DirectorySeparatorChar;

            // Process images.
            if (!Screen.IsFullHeadless) {
                foreach (string file in imageFilePaths) {
                    try {
                        using Stream titleStream = TitleContainer.OpenStream(FileIO.GetRelativePathTo(baseDir, file));
                        string filePath = FormatFilePath(FileIO.GetRelativePathTo(path, file));
                        Texture2D tex = Texture2D.FromStream(gfxDevice, titleStream);

                        // Replace existing item if one exists.
                        streamTextures.Remove(filePath);
                        streamTextures.Add(filePath, tex);
                        streamDisposables.Add(tex);
                    } catch (Exception) { /* ignored */ }
                }
            }

            // TODO: Others...
        }

        private void UnloadStreamContent()
        {
            foreach (IDisposable disposable in streamDisposables) {
                disposable.Dispose();
            }
            streamDisposables.Clear();
            streamTextures.Clear();
        }

        private string FormatFilePath(string filePath) 
            => Path.ChangeExtension(filePath, null)?.Replace('\\', '/');

        public override T Load<T>(string name)
        {
            try {

                // Try to return FromStream() if it exists.
                if (typeof(T) == typeof(Texture2D)) {
                    if (streamTextures.TryGetValue(name, out object tex)) {
                        return (T) tex;
                    }
                }

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
