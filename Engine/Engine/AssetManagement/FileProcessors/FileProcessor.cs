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
    // TODO: Clean this shitty class up. It's fucking terrible and hard to read!
    public abstract class FileProcessor
    {
        public const string _DATA_EXTENSION = ".sdata";

        public readonly string BaseDirectory = FileIO.BaseDirectory;

        private Dictionary<string, string> filePathsExtensions = new Dictionary<string, string>();

        private HashSet<string> filePaths = new HashSet<string>();
        private QuickList<IDisposable> disposables = new QuickList<IDisposable>();
        private Dictionary<string, object> loadedFiles = new Dictionary<string, object>();

        public abstract Type Type { get; }
        public abstract string ContentSubDirectory { get; }
        public abstract string[] AllowedExtensions { get; }
        public string ContentBaseDirectory { get; internal set; }

        internal void ProcessFiles()
        {
            // Check if the sub-directory exists.
            string path = Path.Combine(BaseDirectory, ContentBaseDirectory);
            string subDirectory = Path.Combine(path, ContentSubDirectory);
            if(!Directory.Exists(subDirectory))
                return;

            // Locate unprocessed files.
            string[] allowed = AllowedExtensions;
            foreach (string file in FileIO.GetAllFiles(subDirectory, allowed)) {
                byte[] bytes = File.ReadAllBytes(file);
                string fileNoExtension = Path.ChangeExtension(file, null);
                string extension = Path.GetExtension(file);
                
                // Create processed file.
                using FileStream stream = new FileStream(fileNoExtension + _DATA_EXTENSION, FileMode.OpenOrCreate, FileAccess.Write);
                using BinaryWriter writer = new BinaryWriter(stream);
                stream.SetLength(0);

                // TODO: Allow for manual modification of the header.
                SEFileHeader header = new SEFileHeader(SEFileHeaderFlags.None, 1, extension, new byte[0], (uint) bytes.Length);

                // Write to the processed file, and delete the old file.
                header.WriteToStream(writer);
                writer.Write(bytes);
                File.Delete(file);
            }
        }

        internal void LocateFiles()
        {
            string path = Path.Combine(BaseDirectory, ContentBaseDirectory);
            string subDirectory = Path.Combine(path, ContentSubDirectory);
            if(!Directory.Exists(subDirectory))
                return;

            foreach (string file in FileIO.GetAllFiles(subDirectory, new [] { _DATA_EXTENSION })) {
                filePaths.Add(file);
                string relative = ContentLoader.FormatFilePath(FileIO.GetRelativePathTo(subDirectory, file));
                filePathsExtensions.Add(relative, file);
            }
        }

        internal void LoadFiles(GraphicsDevice gfxDevice)
        {
            string path = Path.Combine(BaseDirectory, ContentBaseDirectory);
            foreach (string file in filePaths) {
                string filePath = ContentLoader.FormatFilePath(FileIO.GetRelativePathTo(path, file));
                if (loadedFiles.ContainsKey(filePath))
                    continue;

                // Load the header and file into memory.
                using Stream titleStream = TitleContainer.OpenStream(FileIO.GetRelativePathTo(BaseDirectory, file));
                using BinaryReader reader = new BinaryReader(titleStream);
                SEFileHeader header = SEFileHeader.ReadFromStream(reader);
                if (!LoadFile(gfxDevice, reader, header, out object obj)) 
                    continue;

                // Add the loaded file to dictionary.
                loadedFiles.Add(filePath, obj);
                if (obj is IDisposable disposable) {
                    disposables.Add(disposable);
                }
            }
        }

        internal bool LoadSingleFile(GraphicsDevice gfxDevice, string fileName, out object obj)
        {
            obj = null;
            if (!filePathsExtensions.TryGetValue(fileName, out fileName))
                return false;

            // Get path, and return if the file is already loaded.
            string path = Path.Combine(BaseDirectory, ContentBaseDirectory);
            string filePath = ContentLoader.FormatFilePath(FileIO.GetRelativePathTo(path, fileName));
            if (loadedFiles.TryGetValue(filePath, out obj))
                return true;

            // If the file isn't loaded, load it into memory.
            using Stream titleStream = TitleContainer.OpenStream(FileIO.GetRelativePathTo(BaseDirectory, fileName));
            using BinaryReader reader = new BinaryReader(titleStream);
            SEFileHeader header = SEFileHeader.ReadFromStream(reader);
            if (!LoadFile(gfxDevice, reader, header, out obj)) 
                return false;

            // Add the loaded file to the dictionary.
            loadedFiles.Add(filePath, obj);
            if (obj is IDisposable disposable) {
                disposables.Add(disposable);
            }
            return true;
        }

        internal void Unload()
        {
            foreach (IDisposable disposable in disposables) {
                disposable.Dispose();
            }
            disposables.Clear();
            loadedFiles.Clear();
        }

        internal bool GetFile(GraphicsDevice gfx, string key, out object file) 
            => loadedFiles.TryGetValue(key, out file) || LoadSingleFile(gfx, key, out file);

        protected abstract bool LoadFile(GraphicsDevice gfxDevice, BinaryReader file, SEFileHeader header, out object obj);
    }
}
