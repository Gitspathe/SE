﻿using System;
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
        internal HashSet<Asset> References = new HashSet<Asset>();
        /// <summary>Holds all asset references, including ones which aren't active.</summary>
        internal HashSet<Asset> AllRefs = new HashSet<Asset>();

        internal HashSet<string> PreloadFiles = new HashSet<string>();

        public bool Inactive => timeInactive >= 5.0f;

        private string rootDirectory;
        private GraphicsDevice gfxDevice;
        private bool loaded;
        private float timeInactive;
        private List<Asset> orderedReferences = new List<Asset>();

        public ContentLoader(IServiceProvider serviceProvider, string id, string rootDirectory) : base(serviceProvider, rootDirectory)
        {
            ID = id;
            this.rootDirectory = rootDirectory;
            if (!Screen.IsFullHeadless) {
                gfxDevice = ((IGraphicsDeviceService) serviceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            }

            AssetManager.AddContentManager(this);
        }

        public override T Load<T>(string name)
        {
            try {
                // First try to load using the FileMarshal.
                string path = FixPath(Path.Combine(rootDirectory, name));
                if (FileMarshal.TryLoad(path, out T file)) {
                    return file;
                }

                // No custom FileProcessor was found, fall back to the MG content manager.
                return base.Load<T>(name);
            } catch (ArgumentNullException e) {
                if (Screen.IsFullHeadless) // No gfx device.
                    throw new HeadlessNotSupportedException("Content could not be loaded in fully headless display mode.", e);

                throw;
            }
        }

        private static string FixPath(string path)
        {
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            path = path.Replace('/', Path.DirectorySeparatorChar);
            int first = path.IndexOf(Path.DirectorySeparatorChar);
            return path.Substring(first+1);
        }

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

            // Tell the FileMarshal to load files in the background.
            foreach (string str in PreloadFiles) {
                string path = FixPath(Path.Combine(rootDirectory, str));
                if(FileMarshal.TryGet(path, out FileMarshal.File file)) {
                    FileMarshal.FlagBackgroundLoad(file);
                }
            }

            // Sort all references.
            orderedReferences.Clear();
            orderedReferences.AddRange(AllRefs);
            orderedReferences.Sort((x, y) => x.LoadOrder.CompareTo(y.LoadOrder));

            // Reload all references in order from lowest to highest LoadOrder.
            // All references are loaded initially in order to build up the dependencies.
            foreach (Asset reference in orderedReferences) {
                reference.Load();
            }

            // Remove unneeded references. TODO: Might cause an exception, investigate.
            foreach (Asset reference in orderedReferences) {
                reference.Purge();
            }
        }

        internal void AddReference(Asset reference)
        {
            References.Add(reference);
            AllRefs.Add(reference);
            if (!loaded && References.Count > 0) {
                Reload();
            }
        }

        internal void RemoveReference(Asset reference)
        {
            References.Remove(reference);

            // Clear stale entries.
            HashSet<Asset> copy = new HashSet<Asset>(References);
            foreach (Asset assetRef in copy) {
                assetRef.Purge();
            }
        }

        public override void Unload()
        {
            loaded = false;

            // Tell the FileMarshal to unload files in the background.
            foreach (string str in PreloadFiles) {
                string path = FixPath(Path.Combine(rootDirectory, str));
                if(FileMarshal.TryGet(path, out FileMarshal.File file)) {
                    FileMarshal.FlagBackgroundUnload(file);
                }
            }

            // Unload all assets from this content loader from memory.
            foreach (Asset asset in AllRefs) {
                asset.Unload();
            }
            base.Unload();
        }
    }
}
