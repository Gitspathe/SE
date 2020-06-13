using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using SE.Core.Exceptions;
using SE.Utility;

namespace SE.AssetManagement.FileProcessors
{
    public abstract class FileProcessor
    {
        public readonly string BaseDirectory = FileIO.BaseDirectory;
       
        private HashSet<string> filePaths = new HashSet<string>();
        private QuickList<IDisposable> disposables = new QuickList<IDisposable>();
        private Dictionary<string, object> loadedFiles = new Dictionary<string, object>();

        public abstract Type Type { get; }
        public abstract string ContentSubDirectory { get; }
        public abstract string[] AllowedExtensions { get; }
        public string ContentBaseDirectory { get; internal set; }

        internal void LocateFilesInternal()
        {
            string path = Path.Combine(BaseDirectory, ContentBaseDirectory);
            string imageDirectory = Path.Combine(path, ContentSubDirectory);
            if(!Directory.Exists(imageDirectory))
                return;

            // Process images.
            string[] allowed = AllowedExtensions;
            foreach (string file in FileIO.GetAllFiles(imageDirectory, allowed)) {
                filePaths.Add(file);
            }
        }

        internal void LoadFilesInternal(GraphicsDevice gfxDevice)
        {
            string path = Path.Combine(BaseDirectory, ContentBaseDirectory);
            foreach (string file in filePaths) {
                string filePath = ContentLoader.FormatFilePath(FileIO.GetRelativePathTo(path, file));
                if (loadedFiles.ContainsKey(filePath))
                    continue;
                if (!LoadFile(gfxDevice, FileIO.GetRelativePathTo(BaseDirectory, file), out object obj)) 
                    continue;

                // Add the loaded file to dictionary.
                loadedFiles.Add(filePath, obj);
                if (obj is IDisposable disposable) {
                    disposables.Add(disposable);
                }
            }
        }

        internal void Unload()
        {
            foreach (IDisposable disposable in disposables) {
                disposable.Dispose();
            }
            disposables.Clear();
            loadedFiles.Clear();
        }

        internal bool GetFile(string key, out object file) 
            => loadedFiles.TryGetValue(key, out file);

        protected abstract bool LoadFile(GraphicsDevice gfxDevice, string file, out object obj);
    }
}
