using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SE.Core;
using SE.Core.Exceptions;
using SE.Editor.Debug.Commands.LevelEdit;
using SE.Utility;

namespace SE.AssetManagement.FileProcessors
{
    public abstract class FileProcessor
    {
        private Dictionary<string, object> loadedFiles = new Dictionary<string, object>();
        
        public abstract Type Type { get; }

        internal bool LoadFileInternal(GraphicsDevice gfxDevice, FileMarshal.File file, out object obj)
        {
            using MemoryStream stream = new MemoryStream(file.Data);
            using BinaryReader reader = new BinaryReader(stream);
            if (LoadFile(gfxDevice, reader, ref file.Header, out obj)) {
                loadedFiles.Add(file.FileName, obj);
                return true;
            }
            return false;
        }

        internal void Unload(string file)
        {
            if (loadedFiles.TryGetValue(file, out object obj)) {
                if (obj is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
            loadedFiles.Remove(file);
        }

        internal bool GetFile(string key, out object file) 
            => loadedFiles.TryGetValue(key, out file);

        protected abstract bool LoadFile(GraphicsDevice gfxDevice, BinaryReader reader, ref SEFileHeader header, out object obj);
    }
}
